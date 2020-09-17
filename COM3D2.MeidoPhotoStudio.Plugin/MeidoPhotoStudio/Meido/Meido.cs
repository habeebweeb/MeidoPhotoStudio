using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using static TBody;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class Meido : ISerializable
    {
        private bool initialized;
        private DragPointGravity hairGravityDragPoint;
        private GravityTransformControl hairGravityControl;
        public bool HairGravityValid => hairGravityControl != null;
        private DragPointGravity skirtGravityDragPoint;
        private GravityTransformControl skirtGravityControl;
        public bool SkirtGravityValid => skirtGravityControl != null;
        private float[] BlendSetValueBackup;
        public const int meidoDataVersion = 1000;
        public static readonly PoseInfo DefaultPose =
            new PoseInfo(Constants.PoseGroupList[0], Constants.PoseDict[Constants.PoseGroupList[0]][0]);
        public static readonly string defaultFaceBlendSet = "通常";
        public static readonly string[] faceKeys = new string[24]
        {
            "eyeclose", "eyeclose2", "eyeclose3", "eyebig", "eyeclose6", "eyeclose5", "hitomih",
            "hitomis", "mayuha", "mayuw", "mayuup", "mayuv", "mayuvhalf", "moutha", "mouths",
            "mouthc", "mouthi", "mouthup", "mouthdw", "mouthhe", "mouthuphalf", "tangout",
            "tangup", "tangopen"
        };

        public static readonly string[] faceToggleKeys = new string[12]
        {
            // blush, shade, nose up, tears, drool, teeth
            "hoho2", "shock", "nosefook", "namida", "yodare", "toothoff",
            // cry 1, cry 2, cry 3, blush 1, blush 2, blush 3
            "tear1", "tear2", "tear3", "hohos", "hoho", "hohol"
        };
        public enum Curl
        {
            front, back, shift
        }
        public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;
        public int StockNo { get; }
        public Maid Maid { get; }
        public TBody Body => Maid.body0;
        public MeidoDragPointManager IKManager { get; }
        public Texture2D Portrait { get; }
        public PoseInfo CachedPose { get; private set; } = DefaultPose;
        public string CurrentFaceBlendSet { get; private set; } = defaultFaceBlendSet;
        public int Slot { get; private set; }
        public bool Loading { get; private set; }
        public string FirstName => Maid.status.firstName;
        public string LastName => Maid.status.lastName;
        public bool Busy => Maid.IsBusy && Loading;
        public bool CurlingFront => Maid.IsItemChange("skirt", "めくれスカート")
            || Maid.IsItemChange("onepiece", "めくれスカート");
        public bool CurlingBack => Maid.IsItemChange("skirt", "めくれスカート後ろ")
            || Maid.IsItemChange("onepiece", "めくれスカート後ろ");
        public bool PantsuShift => Maid.IsItemChange("panz", "パンツずらし")
            || Maid.IsItemChange("mizugi", "パンツずらし");
        private bool freeLook;
        public bool FreeLook
        {
            get => freeLook;
            set
            {
                if (freeLook == value) return;
                freeLook = value;
                Body.trsLookTarget = freeLook ? null : GameMain.Instance.MainCamera.transform;
                OnUpdateMeido();
            }
        }
        public bool HeadToCam
        {
            get => Body.isLoadedBody && Body.boHeadToCam;
            set
            {
                if (!Body.isLoadedBody || HeadToCam == value) return;
                Body.HeadToCamPer = 0f;
                Body.boHeadToCam = value;
                if (!HeadToCam && !EyeToCam) FreeLook = false;
                OnUpdateMeido();
            }
        }
        public bool EyeToCam
        {
            get => Body.isLoadedBody && Body.boEyeToCam;
            set
            {
                if (!Body.isLoadedBody || EyeToCam == value) return;
                Body.boEyeToCam = value;
                if (!HeadToCam && !EyeToCam) FreeLook = false;
                OnUpdateMeido();
            }
        }
        public bool Stop
        {
            get => !Body.isLoadedBody || !Maid.GetAnimation().isPlaying;
            set
            {
                if (!Body.isLoadedBody || value == Stop) return;
                if (value) Maid.GetAnimation().Stop();
                else
                {
                    Body.boEyeToCam = true;
                    Body.boHeadToCam = true;
                    SetPose(CachedPose.Pose);
                }

                OnUpdateMeido();
            }
        }
        public bool IK
        {
            get => IKManager.Active;
            set
            {
                if (value == IKManager.Active) return;

                IKManager.Active = value;
            }
        }
        public bool Bone
        {
            get => IKManager.IsBone;
            set
            {
                if (value == Bone) return;

                IKManager.IsBone = value;
                OnUpdateMeido();
            }
        }
        public bool HairGravityActive
        {
            get => HairGravityValid && hairGravityDragPoint.gameObject.activeSelf;
            set
            {
                if (HairGravityValid && value != HairGravityActive)
                {
                    hairGravityDragPoint.gameObject.SetActive(value);
                    hairGravityControl.isEnabled = value;
                }
            }
        }
        public bool SkirtGravityActive
        {
            get => SkirtGravityValid && skirtGravityDragPoint.gameObject.activeSelf;
            set
            {
                if (SkirtGravityValid && value != SkirtGravityActive)
                {
                    skirtGravityDragPoint.gameObject.SetActive(value);
                    skirtGravityControl.isEnabled = value;
                }
            }
        }
        public event EventHandler<GravityEventArgs> GravityMove;
        public Quaternion DefaultEyeRotL { get; private set; }
        public Quaternion DefaultEyeRotR { get; private set; }

        public Meido(int stockMaidIndex)
        {
            StockNo = stockMaidIndex;
            Maid = GameMain.Instance.CharacterMgr.GetStockMaid(stockMaidIndex);
            Portrait = Maid.GetThumIcon();
            IKManager = new MeidoDragPointManager(this);
            IKManager.SelectMaid += (s, args) => OnUpdateMeido(args);
        }

        public void BeginLoad()
        {
            FreeLook = false;
            Maid.Visible = true;
            Body.boHeadToCam = true;
            Body.boEyeToCam = true;
            Body.SetBoneHitHeightY(-1000f);
        }

        public void Load(int slot)
        {
            Slot = slot;
            Loading = true;

            if (!Body.isLoadedBody)
            {
                Maid.DutPropAll();
                Maid.AllProcPropSeqStart();
            }

            GameMain.Instance.StartCoroutine(Load());
        }

        private IEnumerator Load()
        {
            while (Maid.IsBusy) yield return null;

            yield return new WaitForEndOfFrame();

            OnBodyLoad();
        }

        public void Unload()
        {
            if (Body.isLoadedBody)
            {
                DetachAllMpnAttach();
                Body.jbMuneL.enabled = true;
                Body.jbMuneR.enabled = true;

                Body.quaDefEyeL = DefaultEyeRotL;
                Body.quaDefEyeR = DefaultEyeRotR;

                HairGravityActive = false;
                SkirtGravityActive = false;

                if (HairGravityValid) hairGravityDragPoint.Move -= OnGravityEvent;
                if (SkirtGravityValid) skirtGravityDragPoint.Move -= OnGravityEvent;
            }

            Body.MuneYureL(1f);
            Body.MuneYureR(1f);

            Body.SetMaskMode(MaskMode.None);
            Body.SetBoneHitHeightY(0f);

            Maid.Visible = false;

            IKManager.Destroy();
        }

        public void Deactivate()
        {
            if (Body.isLoadedBody) SetFaceBlendSet(defaultFaceBlendSet);

            Unload();

            DestroyGravityControl(ref hairGravityControl);
            DestroyGravityControl(ref skirtGravityControl);
            GameObject.Destroy(hairGravityDragPoint?.gameObject);
            GameObject.Destroy(skirtGravityDragPoint?.gameObject);

            Maid.SetPos(Vector3.zero);
            Maid.SetRot(Vector3.zero);
            Maid.SetPosOffset(Vector3.zero);
            Body.transform.localScale = Vector3.one;
            Maid.ResetAll();
            Maid.MabatakiUpdateStop = false;
            Maid.ActiveSlotNo = -1;
        }

        public void SetPose(PoseInfo poseInfo)
        {
            CachedPose = poseInfo;
            SetPose(poseInfo.Pose);
        }

        public void SetPose(string pose)
        {
            if (!Body.isLoadedBody) return;

            if (pose.StartsWith(Constants.customPosePath))
            {
                byte[] poseBuffer = File.ReadAllBytes(pose);
                string hash = Path.GetFileName(pose).GetHashCode().ToString();
                Body.CrossFade(hash, poseBuffer, loop: true, fade: 0f);
            }
            else
            {
                string[] poseComponents = pose.Split(',');
                pose = poseComponents[0] + ".anm";

                Maid.CrossFade(pose, loop: true, val: 0f);
                Maid.GetAnimation().Play();

                if (poseComponents.Length > 1)
                {
                    Maid.GetAnimation()[pose].time = float.Parse(poseComponents[1]);
                    Maid.GetAnimation()[pose].speed = 0f;
                }
            }

            Maid.SetAutoTwistAll(true);
            SetMune();
        }

        public void CopyPose(Meido fromMeido)
        {
            byte[] poseBinary = fromMeido.SerializePose();
            string tag = $"copy_{fromMeido.Maid.status.guid}";
            Body.CrossFade(tag, poseBinary, false, true, false, 0f);
            Maid.SetAutoTwistAll(true);
            Maid.transform.rotation = fromMeido.Maid.transform.rotation;
            SetMune();
        }

        public void SetMune(bool drag = false)
        {
            bool momiOrPaizuri = CachedPose.Pose.Contains("_momi") || CachedPose.Pose.Contains("paizuri_");
            float onL = (drag || momiOrPaizuri) ? 0f : 1f;
            Body.MuneYureL(onL);
            Body.MuneYureR(onL);
            Body.jbMuneL.enabled = !drag;
            Body.jbMuneR.enabled = !drag;
        }

        public void SetHandPreset(string filename, bool right)
        {
            XDocument handDocument = XDocument.Load(filename);
            XElement handElement = handDocument.Element("FingerData");
            if (handElement.IsEmpty || handElement.Element("GameVersion").IsEmpty
                || handElement.Element("RightData").IsEmpty || handElement.Element("BinaryData").IsEmpty
            ) return;

            Stop = true;

            bool rightData = bool.Parse(handElement.Element("RightData").Value);
            string base64Data = handElement.Element("BinaryData").Value;

            byte[] handData = Convert.FromBase64String(base64Data);

            IKManager.DeserializeHand(handData, right, rightData != right);
        }

        public byte[] SerializePose(bool frameBinary = false)
        {
            CacheBoneDataArray cache = GetCacheBoneData();
            return frameBinary ? cache.GetFrameBinary(true, true) : cache.GetAnmBinary(true, true);
        }

        public Dictionary<string, float> SerializeFace()
        {
            Dictionary<string, float> faceData = new Dictionary<string, float>();
            foreach (string hash in faceKeys.Concat(faceToggleKeys))
            {
                try
                {
                    float value = GetFaceBlendValue(hash);
                    faceData.Add(hash, value);
                }
                catch { }
            }

            return faceData;
        }

        public void SetFaceBlendSet(string blendSet, bool custom = false)
        {
            if (custom)
            {
                XDocument faceDocument = XDocument.Load(blendSet);
                XElement faceDataElement = faceDocument.Element("FaceData");
                if (faceDataElement.IsEmpty) return;

                HashSet<string> hashKeys = new HashSet<string>(faceKeys.Concat(faceToggleKeys));

                foreach (XElement element in faceDataElement.Elements())
                {
                    string key;
                    if ((key = (string)element.Attribute("name")) != null && !hashKeys.Contains(key)) continue;

                    if (float.TryParse(element.Value, out float value))
                    {
                        try
                        {
                            SetFaceBlendValue(key, value);
                        }
                        catch { }
                    }
                }
            }
            else
            {
                ApplyBackupBlendSet();

                CurrentFaceBlendSet = blendSet;

                BackupBlendSetValuess();

                Maid.FaceAnime(blendSet, 0f);
            }

            StopBlink();
            OnUpdateMeido();
        }

        public void SetFaceBlendValue(string hash, float value)
        {
            TMorph morph = Body.Face.morph;
            if (hash == "nosefook") Maid.boNoseFook = morph.boNoseFook = value > 0f;
            else
            {
                hash = Utility.GP01FbFaceHash(morph, hash);
                try
                {
                    morph.dicBlendSet[CurrentFaceBlendSet][(int)morph.hash[hash]] = value;
                }
                catch { }
            }
        }

        public float GetFaceBlendValue(string hash)
        {
            TMorph morph = Body.Face.morph;
            if (hash == "nosefook") return (Maid.boNoseFook || morph.boNoseFook) ? 1f : 0f;
            hash = Utility.GP01FbFaceHash(morph, hash);
            return morph.dicBlendSet[CurrentFaceBlendSet][(int)morph.hash[hash]];
        }

        public void StopBlink()
        {
            Maid.MabatakiUpdateStop = true;
            Body.Face.morph.EyeMabataki = 0f;
            Utility.SetFieldValue(Maid, "MabatakiVal", 0f);
        }

        public void SetMaskMode(MaskMode maskMode)
        {
            bool invisibleBody = !Body.GetMask(SlotID.body);
            Body.SetMaskMode(maskMode);
            if (invisibleBody) SetBodyMask(false);
        }

        public void SetBodyMask(bool enabled)
        {
            Hashtable table = Utility.GetFieldValue<TBody, Hashtable>(Body, "m_hFoceHide");
            foreach (SlotID bodySlot in MaidDressingPane.bodySlots)
            {
                table[bodySlot] = enabled;
            }
            if (Body.goSlot[19].m_strModelFileName.Contains("melala_body"))
            {
                table[SlotID.accHana] = enabled;
            }
            Body.FixMaskFlag();
            Body.FixVisibleFlag(false);
        }

        public void SetCurling(Curl curling, bool enabled)
        {
            string[] name = curling == Curl.shift
                ? new[] { "panz", "mizugi" }
                : new[] { "skirt", "onepiece" };
            if (enabled)
            {
                string action = curling == Curl.shift
                    ? "パンツずらし" : curling == Curl.front
                        ? "めくれスカート" : "めくれスカート後ろ";
                Maid.ItemChangeTemp(name[0], action);
                Maid.ItemChangeTemp(name[1], action);
            }
            else
            {
                Maid.ResetProp(name[0]);
                Maid.ResetProp(name[1]);
            }
            Maid.AllProcProp();
            hairGravityControl?.OnChangeMekure();
            skirtGravityControl?.OnChangeMekure();
        }

        public void SetMpnProp(MpnAttachProp prop, bool detach)
        {
            if (detach) Maid.ResetProp(prop.Tag, false);
            else Maid.SetProp(prop.Tag, prop.MenuFile, 0, true);
            Maid.AllProcProp();
        }

        public void DetachAllMpnAttach()
        {
            Maid.ResetProp(MPN.kousoku_lower, false);
            Maid.ResetProp(MPN.kousoku_upper, false);
            Maid.AllProcProp();
        }

        public void ApplyGravity(Vector3 position, bool skirt = false)
        {
            DragPointGravity dragPoint = skirt ? skirtGravityDragPoint : hairGravityDragPoint;
            if (dragPoint != null) dragPoint.MyObject.localPosition = position;
        }

        private void BackupBlendSetValuess()
        {
            float[] values = Body.Face.morph.dicBlendSet[CurrentFaceBlendSet];
            BlendSetValueBackup = new float[values.Length];
            values.CopyTo(BlendSetValueBackup, 0);
        }

        private void ApplyBackupBlendSet()
        {
            BlendSetValueBackup.CopyTo(Body.Face.morph.dicBlendSet[CurrentFaceBlendSet], 0);
            Maid.boNoseFook = false;
        }

        private CacheBoneDataArray GetCacheBoneData()
        {
            CacheBoneDataArray cache = Maid.gameObject.GetComponent<CacheBoneDataArray>();
            if (cache == null)
            {
                cache = Maid.gameObject.AddComponent<CacheBoneDataArray>();
                cache.CreateCache(Body.GetBone("Bip01"));
            }
            return cache;
        }

        private void OnBodyLoad()
        {
            if (!initialized)
            {
                TMorph faceMorph = Body.Face.morph;
                DefaultEyeRotL = Body.quaDefEyeL;
                DefaultEyeRotR = Body.quaDefEyeR;

                InitializeGravityControls();

                initialized = true;
            }

            if (BlendSetValueBackup == null) BackupBlendSetValuess();

            if (HairGravityValid) hairGravityDragPoint.Move += OnGravityEvent;
            if (SkirtGravityValid) skirtGravityDragPoint.Move += OnGravityEvent;

            IKManager.Initialize();

            IK = true;
            Stop = false;
            Bone = false;
            Loading = false;
        }

        private void InitializeGravityControls()
        {
            hairGravityControl = InitializeGravityControl("hair");
            if (hairGravityControl.isValid)
            {
                hairGravityDragPoint = MakeGravityDragPoint(hairGravityControl);
                HairGravityActive = false;
            }
            else DestroyGravityControl(ref hairGravityControl);

            skirtGravityControl = InitializeGravityControl("skirt");
            if (skirtGravityControl.isValid)
            {
                skirtGravityDragPoint = MakeGravityDragPoint(skirtGravityControl);
                SkirtGravityActive = false;
            }
            else DestroyGravityControl(ref skirtGravityControl);
        }

        private DragPointGravity MakeGravityDragPoint(GravityTransformControl control)
        {
            DragPointGravity gravityDragpoint = DragPoint.Make<DragPointGravity>(
                PrimitiveType.Cube, Vector3.one * 0.12f
            );
            gravityDragpoint.Initialize(() => control.transform.position, () => Vector3.zero);
            gravityDragpoint.Set(control.transform);

            return gravityDragpoint;
        }

        private GravityTransformControl InitializeGravityControl(string category)
        {
            Transform bone = Body.GetBone("Bip01");
            string gravityGoName = $"GravityDatas_{Maid.status.guid}_{category}";
            Transform gravityTransform = Maid.gameObject.transform.Find(gravityGoName);
            if (gravityTransform == null)
            {
                GameObject go = new GameObject(gravityGoName);
                go.transform.SetParent(bone, false);
                go.transform.SetParent(Maid.transform, true);
                go.transform.localScale = Vector3.one;
                go.transform.rotation = Quaternion.identity;
                GameObject go2 = new GameObject(gravityGoName);
                go2.transform.SetParent(go.transform, false);
                gravityTransform = go2.transform;
            }
            else
            {
                gravityTransform = gravityTransform.GetChild(0);
                GravityTransformControl control = gravityTransform.GetComponent<GravityTransformControl>();
                if (control != null) GameObject.Destroy(control);
            }

            GravityTransformControl gravityControl = gravityTransform.gameObject.AddComponent<GravityTransformControl>();

            SlotID[] slots = category == "skirt"
                ? new[] { SlotID.skirt, SlotID.onepiece, SlotID.mizugi, SlotID.panz }
                : new[] { SlotID.hairF, SlotID.hairR, SlotID.hairS, SlotID.hairT };

            gravityControl.SetTargetSlods(slots);
            gravityControl.forceRate = 0.1f;

            return gravityControl;
        }

        private void DestroyGravityControl(ref GravityTransformControl control)
        {
            if (control != null)
            {
                GameObject.Destroy(control.transform.parent.gameObject);
                control = null;
            }
        }

        private void OnUpdateMeido(MeidoUpdateEventArgs args = null)
        {
            UpdateMeido?.Invoke(this, args ?? MeidoUpdateEventArgs.Empty);
        }

        private void OnGravityEvent(object sender, EventArgs args) => OnGravityChange((DragPointGravity)sender);

        private void OnGravityChange(DragPointGravity dragPoint)
        {
            GravityEventArgs args = new GravityEventArgs(
                dragPoint == skirtGravityDragPoint, dragPoint.MyObject.transform.localPosition
            );
            GravityMove?.Invoke(this, args);
        }

        public void Serialize(BinaryWriter binaryWriter)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter tempWriter = new BinaryWriter(memoryStream))
            {
                // transform
                tempWriter.WriteVector3(Maid.transform.position);
                tempWriter.WriteQuaternion(Maid.transform.rotation);
                tempWriter.WriteVector3(Maid.transform.localScale);
                // pose
                byte[] poseBuffer = SerializePose(true);
                tempWriter.Write(poseBuffer.Length);
                tempWriter.Write(poseBuffer);
                CachedPose.Serialize(tempWriter);
                // eye direction
                tempWriter.WriteQuaternion(Body.quaDefEyeL * Quaternion.Inverse(DefaultEyeRotL));
                tempWriter.WriteQuaternion(Body.quaDefEyeR * Quaternion.Inverse(DefaultEyeRotR));
                // free look
                tempWriter.Write(FreeLook);
                if (FreeLook)
                {
                    tempWriter.WriteVector3(Body.offsetLookTarget);
                    tempWriter.WriteVector3(Utility.GetFieldValue<TBody, Vector3>(Body, "HeadEulerAngle"));
                }
                // Head/eye to camera
                tempWriter.Write(HeadToCam);
                tempWriter.Write(EyeToCam);
                // face
                SerializeFace(tempWriter);
                // body visible
                tempWriter.Write(Body.GetMask(SlotID.body));
                // clothing
                foreach (SlotID clothingSlot in MaidDressingPane.clothingSlots)
                {
                    bool value = true;
                    if (clothingSlot == SlotID.wear)
                    {
                        if (MaidDressingPane.wearSlots.Any(slot => Body.GetSlotLoaded(slot)))
                        {
                            value = MaidDressingPane.wearSlots.Any(slot => Body.GetMask(slot));
                        }
                    }
                    else if (clothingSlot == SlotID.megane)
                    {
                        SlotID[] slots = new[] { SlotID.megane, SlotID.accHead };
                        if (slots.Any(slot => Body.GetSlotLoaded(slot)))
                        {
                            value = slots.Any(slot => Body.GetMask(slot));
                        }
                    }
                    else if (Body.GetSlotLoaded(clothingSlot))
                    {
                        value = Body.GetMask(clothingSlot);
                    }
                    tempWriter.Write(value);
                }
                // zurashi and mekure
                tempWriter.Write(CurlingFront);
                tempWriter.Write(CurlingBack);
                tempWriter.Write(PantsuShift);

                bool hasKousokuUpper = Body.GetSlotLoaded(SlotID.kousoku_upper);
                tempWriter.Write(hasKousokuUpper);
                if (hasKousokuUpper) tempWriter.Write(Maid.GetProp(MPN.kousoku_upper).strTempFileName);

                bool hasKousokuLower = Body.GetSlotLoaded(SlotID.kousoku_lower);
                tempWriter.Write(hasKousokuLower);
                if (hasKousokuLower) tempWriter.Write(Maid.GetProp(MPN.kousoku_lower).strTempFileName);

                binaryWriter.Write(memoryStream.Length);
                binaryWriter.Write(memoryStream.ToArray());
            }
        }

        private void SerializeFace(BinaryWriter binaryWriter)
        {
            binaryWriter.Write("MPS_FACE");
            foreach (string hash in faceKeys.Concat(faceToggleKeys))
            {
                try
                {
                    float value = GetFaceBlendValue(hash);
                    binaryWriter.Write(hash);
                    binaryWriter.Write(value);
                }
                catch { }
            }
            binaryWriter.Write("END_FACE");
        }

        public void Deserialize(BinaryReader binaryReader) => Deserialize(binaryReader, meidoDataVersion, false);

        public void Deserialize(BinaryReader binaryReader, int dataVersion, bool mmScene)
        {
            Maid.GetAnimation().Stop();
            DetachAllMpnAttach();

            binaryReader.ReadInt64(); // meido buffer length
            // transform
            Maid.transform.position = binaryReader.ReadVector3();
            Maid.transform.rotation = binaryReader.ReadQuaternion();
            Maid.transform.localScale = binaryReader.ReadVector3();
            // pose
            if (mmScene) IKManager.Deserialize(binaryReader);
            else
            {
                int poseBufferLength = binaryReader.ReadInt32();
                byte[] poseBuffer = binaryReader.ReadBytes(poseBufferLength);
                GetCacheBoneData().SetFrameBinary(poseBuffer);
            }

            Body.MuneYureL(0f);
            Body.MuneYureR(0f);
            Body.jbMuneL.enabled = false;
            Body.jbMuneR.enabled = false;

            CachedPose = PoseInfo.Deserialize(binaryReader);
            // eye direction
            Body.quaDefEyeL = binaryReader.ReadQuaternion() * DefaultEyeRotL;
            Body.quaDefEyeR = binaryReader.ReadQuaternion() * DefaultEyeRotR;
            // free look
            FreeLook = binaryReader.ReadBoolean();
            if (FreeLook)
            {
                Body.offsetLookTarget = binaryReader.ReadVector3();
                // Head angle cannot be resolved with just the offsetLookTarget
                if (!mmScene)
                {
                    Utility.SetFieldValue(Body, "HeadEulerAngleG", Vector3.zero);
                    Utility.SetFieldValue(Body, "HeadEulerAngle", binaryReader.ReadVector3());
                }
            }
            // Head/eye to camera
            HeadToCam = binaryReader.ReadBoolean();
            EyeToCam = binaryReader.ReadBoolean();
            // face
            DeserializeFace(binaryReader);
            // body visible
            SetBodyMask(binaryReader.ReadBoolean());
            // clothing
            foreach (SlotID clothingSlot in MaidDressingPane.clothingSlots)
            {
                bool value = binaryReader.ReadBoolean();
                if (mmScene) continue;
                if (clothingSlot == SlotID.wear)
                {
                    Body.SetMask(SlotID.wear, value);
                    Body.SetMask(SlotID.mizugi, value);
                    Body.SetMask(SlotID.onepiece, value);
                }
                else if (clothingSlot == SlotID.megane)
                {
                    Body.SetMask(SlotID.megane, value);
                    Body.SetMask(SlotID.accHead, value);
                }
                else if (Body.GetSlotLoaded(clothingSlot))
                {
                    Body.SetMask(clothingSlot, value);
                }
            }
            // zurashi and mekure
            bool curlingFront = binaryReader.ReadBoolean();
            bool curlingBack = binaryReader.ReadBoolean();
            bool curlingPantsu = binaryReader.ReadBoolean();
            if (!mmScene)
            {
                if (CurlingFront != curlingFront) SetCurling(Curl.front, curlingFront);
                if (CurlingBack != curlingBack) SetCurling(Curl.back, curlingBack);
                SetCurling(Curl.shift, curlingPantsu);
            }

            bool hasKousokuUpper = binaryReader.ReadBoolean();
            if (hasKousokuUpper)
            {
                try
                {
                    SetMpnProp(new MpnAttachProp(MPN.kousoku_upper, binaryReader.ReadString()), false);
                }
                catch { }
            }

            bool hasKousokuLower = binaryReader.ReadBoolean();
            if (hasKousokuLower)
            {
                try
                {
                    SetMpnProp(new MpnAttachProp(MPN.kousoku_lower, binaryReader.ReadString()), false);
                }
                catch { }
            }
            // OnUpdateMeido();
        }

        private void DeserializeFace(BinaryReader binaryReader)
        {
            StopBlink();
            binaryReader.ReadString(); // read face header
            string header;
            while ((header = binaryReader.ReadString()) != "END_FACE")
            {
                SetFaceBlendValue(header, binaryReader.ReadSingle());
            }
        }
    }

    public class GravityEventArgs : EventArgs
    {
        public Vector3 LocalPosition { get; }
        public bool IsSkirt { get; }

        public GravityEventArgs(bool isSkirt, Vector3 localPosition)
        {
            LocalPosition = localPosition;
            IsSkirt = isSkirt;
        }
    }

    public struct PoseInfo
    {
        public string PoseGroup { get; }
        public string Pose { get; }
        public bool CustomPose { get; }
        public PoseInfo(string poseGroup, string pose, bool customPose = false)
        {
            PoseGroup = poseGroup;
            Pose = pose;
            CustomPose = customPose;
        }

        public void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(PoseGroup);
            binaryWriter.Write(Pose);
            binaryWriter.Write(CustomPose);
        }

        public static PoseInfo Deserialize(BinaryReader binaryReader)
        {
            return new PoseInfo
            (
                binaryReader.ReadString(),
                binaryReader.ReadString(),
                binaryReader.ReadBoolean()
            );
        }
    }
}
