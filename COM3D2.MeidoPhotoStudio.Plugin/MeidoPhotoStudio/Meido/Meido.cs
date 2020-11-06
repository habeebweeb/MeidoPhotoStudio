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
    public class Meido : ISerializable
    {
        private bool initialized;
        private float[] BlendSetValueBackup;
        public DragPointGravity HairGravityControl { get; private set; }
        public DragPointGravity SkirtGravityControl { get; private set; }
        public bool HairGravityActive
        {
            get => HairGravityControl.Valid && HairGravityControl.Active;
            set
            {
                if (HairGravityControl.Valid && value != HairGravityControl.Active)
                {
                    HairGravityControl.gameObject.SetActive(value);
                }
            }
        }
        public bool SkirtGravityActive
        {
            get => SkirtGravityControl.Valid && SkirtGravityControl.Active;
            set
            {
                if (SkirtGravityControl.Valid && value != SkirtGravityControl.Active)
                {
                    SkirtGravityControl.gameObject.SetActive(value);
                }
            }
        }
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
        public Texture2D Portrait => Maid.GetThumIcon();
        public bool IsEditMaid { get; set; }
        public PoseInfo CachedPose { get; private set; } = DefaultPose;
        public string CurrentFaceBlendSet { get; private set; } = defaultFaceBlendSet;
        public int Slot { get; private set; }
        public bool Loading { get; private set; }
        public string FirstName => Maid.status.firstName;
        public string LastName => Maid.status.lastName;
        public bool Busy => Maid.IsBusy || Loading;
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
        public event EventHandler<GravityEventArgs> GravityMove;
        public Quaternion DefaultEyeRotL { get; private set; }
        public Quaternion DefaultEyeRotR { get; private set; }

        public Meido(int stockMaidIndex)
        {
            StockNo = stockMaidIndex;
            Maid = GameMain.Instance.CharacterMgr.GetStockMaid(stockMaidIndex);
            IKManager = new MeidoDragPointManager(this);
            IKManager.SelectMaid += (s, args) => OnUpdateMeido(args);
        }

        public void Load(int slot)
        {
            if (Busy) return;

            Slot = slot;

            FreeLook = false;
            Maid.Visible = true;
            Body.boHeadToCam = true;
            Body.boEyeToCam = true;
            Body.SetBoneHitHeightY(-1000f);

            if (!Body.isLoadedBody)
            {
                Maid.DutPropAll();
                Maid.AllProcPropSeqStart();
            }

            StartLoad(OnBodyLoad);
        }

        private void StartLoad(Action callback)
        {
            if (Loading) return;
            GameMain.Instance.StartCoroutine(Load(callback));
        }

        private IEnumerator Load(Action callback)
        {
            Loading = true;
            while (Maid.IsBusy) yield return null;
            yield return new WaitForEndOfFrame();
            callback();
            Loading = false;
        }

        private void OnBodyLoad()
        {
            if (!initialized)
            {
                DefaultEyeRotL = Body.quaDefEyeL;
                DefaultEyeRotR = Body.quaDefEyeR;

                initialized = true;
            }

            if (BlendSetValueBackup == null) BackupBlendSetValues();

            if (!HairGravityControl) InitializeGravityControls();

            HairGravityControl.Move += OnGravityEvent;
            SkirtGravityControl.Move += OnGravityEvent;
            if (MeidoPhotoStudio.EditMode) AllProcPropSeqStartPatcher.SequenceStart += ReinitializeBody;

            IKManager.Initialize();

            IK = true;
            Stop = false;
            Bone = false;
        }

        private void ReinitializeBody(object sender, ProcStartEventArgs args)
        {
            if (Loading || !Body.isLoadedBody) return;

            if (args.maid.status.guid == Maid.status.guid)
            {
                MPN[] gravityControlProps = new[] {
                    MPN.skirt, MPN.onepiece, MPN.mizugi, MPN.panz, MPN.set_maidwear, MPN.set_mywear, MPN.set_underwear,
                    MPN.hairf, MPN.hairr, MPN.hairs, MPN.hairt
                };

                // Change body
                if (Maid.GetProp(MPN.body).boDut)
                {
                    IKManager.Destroy();
                    StartLoad(reinitializeBody);
                }
                // Change face
                else if (Maid.GetProp(MPN.head).boDut)
                {
                    SetFaceBlendSet(defaultFaceBlendSet);
                    StartLoad(reinitializeFace);
                }
                // Gravity control clothing/hair change
                else if (gravityControlProps.Any(prop => Maid.GetProp(prop).boDut))
                {
                    if (HairGravityControl) GameObject.Destroy(HairGravityControl.gameObject);
                    if (SkirtGravityControl) GameObject.Destroy(SkirtGravityControl.gameObject);

                    StartLoad(reinitializeGravity);
                }
                // Clothing/accessory changes
                // Includes null_mpn too but any button click results in null_mpn bodut I think
                else StartLoad(() => OnUpdateMeido());

                void reinitializeBody()
                {
                    IKManager.Initialize();
                    Stop = false;

                    // Maid animation needs to be set again for custom parts edit
                    GameObject uiRoot = GameObject.Find("UI Root");

                    var customPartsWindow = UTY.GetChildObject(uiRoot, "Window/CustomPartsWindow")
                        .GetComponent<SceneEditWindow.CustomPartsWindow>();

                    Utility.SetFieldValue(customPartsWindow, "animation", Maid.GetAnimation());
                }

                void reinitializeFace()
                {
                    DefaultEyeRotL = Body.quaDefEyeL;
                    DefaultEyeRotR = Body.quaDefEyeR;
                    BackupBlendSetValues();
                }

                void reinitializeGravity()
                {
                    InitializeGravityControls();
                    OnUpdateMeido();
                }
            }
        }

        public void Unload()
        {
            if (Body.isLoadedBody && Maid.Visible)
            {
                DetachAllMpnAttach();
                Body.jbMuneL.enabled = true;
                Body.jbMuneR.enabled = true;

                Body.quaDefEyeL = DefaultEyeRotL;
                Body.quaDefEyeR = DefaultEyeRotR;

                if (HairGravityControl)
                {
                    HairGravityControl.Move -= OnGravityEvent;
                    HairGravityActive = false;
                }

                if (SkirtGravityControl)
                {
                    SkirtGravityControl.Move -= OnGravityEvent;
                    SkirtGravityActive = false;
                }

                ApplyGravity(Vector3.zero, skirt: false);
                ApplyGravity(Vector3.zero, skirt: true);

                SetFaceBlendSet(defaultFaceBlendSet);
            }

            AllProcPropSeqStartPatcher.SequenceStart -= ReinitializeBody;

            Body.MuneYureL(1f);
            Body.MuneYureR(1f);

            Body.SetMaskMode(MaskMode.None);
            Body.SetBoneHitHeightY(0f);

            Maid.Visible = false;

            IKManager.Destroy();
        }

        public void Deactivate()
        {
            Unload();

            if (HairGravityControl) GameObject.Destroy(HairGravityControl.gameObject);
            if (SkirtGravityControl) GameObject.Destroy(SkirtGravityControl.gameObject);

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
                string poseFilename = Path.GetFileNameWithoutExtension(pose);
                try
                {
                    byte[] poseBuffer = File.ReadAllBytes(pose);
                    string hash = Path.GetFileName(pose).GetHashCode().ToString();
                    Body.CrossFade(hash, poseBuffer, loop: true, fade: 0f);
                }
                catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    Utility.LogWarning($"{poseFilename}: Could not open because {e.Message}");
                    Constants.InitializeCustomPoses();
                    return;
                }
                catch (Exception e)
                {
                    Utility.LogWarning($"{poseFilename}: Could not apply pose because {e.Message}");
                    return;
                }
                SetMune(true, left: true);
                SetMune(true, left: false);
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
                SetPoseMune();
            }

            Maid.SetAutoTwistAll(true);
        }

        public void CopyPose(Meido fromMeido)
        {
            Stop = true;
            GetCacheBoneData().SetFrameBinary(fromMeido.SerializePose(frameBinary: true));
            SetMune(fromMeido.Body.GetMuneYureL() != 0f, left: true);
            SetMune(fromMeido.Body.GetMuneYureR() != 0f, left: false);
        }

        public void SetMune(bool enabled, bool left = false)
        {
            float value = enabled ? 1f : 0f;
            if (left)
            {
                Body.MuneYureL(value);
                Body.jbMuneL.enabled = enabled;
            }
            else
            {
                Body.MuneYureR(value);
                Body.jbMuneR.enabled = enabled;
            }
        }

        private void SetPoseMune()
        {
            bool momiOrPaizuri = CachedPose.Pose.Contains("_momi") || CachedPose.Pose.Contains("paizuri_");
            SetMune(!momiOrPaizuri, left: true);
            SetMune(!momiOrPaizuri, left: false);
        }

        public void SetHandPreset(string filename, bool right)
        {
            string faceFilename = Path.GetFileNameWithoutExtension(filename);
            try
            {
                XDocument handDocument = XDocument.Load(filename);
                XElement handElement = handDocument.Element("FingerData");

                if (handElement?.Elements().Any(element => element?.IsEmpty ?? true) ?? true)
                {
                    Utility.LogWarning($"{faceFilename}: Could not apply hand preset because it is invalid.");
                    return;
                }

                Stop = true;

                bool rightData = bool.Parse(handElement.Element("RightData").Value);
                string base64Data = handElement.Element("BinaryData").Value;

                byte[] handData = Convert.FromBase64String(base64Data);

                IKManager.DeserializeHand(handData, right, rightData != right);
            }
            catch (System.Xml.XmlException e)
            {
                Utility.LogWarning($"{faceFilename}: Hand preset data is malformed because {e.Message}");
            }
            catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
            {
                Utility.LogWarning($"{faceFilename}: Could not open hand preset because {e.Message}");
                Constants.InitializeHandPresets();
            }
            catch (Exception e)
            {
                Utility.LogWarning($"{faceFilename}: Could not parse hand preset because {e.Message}");
            }
        }

        public byte[] SerializePose(bool frameBinary = false)
        {
            CacheBoneDataArray cache = GetCacheBoneData();
            bool muneL = Body.GetMuneYureL() == 0f;
            bool muneR = Body.GetMuneYureR() == 0f;
            return frameBinary ? cache.GetFrameBinary(muneL, muneR) : cache.GetAnmBinary(true, true);
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

        public void SetFaceBlendSet(string blendSet)
        {
            if (blendSet.StartsWith(Constants.customFacePath))
            {
                string blendSetFileName = Path.GetFileNameWithoutExtension(blendSet);
                try
                {
                    XDocument faceDocument = XDocument.Load(blendSet, LoadOptions.SetLineInfo);
                    XElement faceDataElement = faceDocument.Element("FaceData");
                    if (faceDataElement?.IsEmpty ?? true)
                    {
                        Utility.LogWarning($"{blendSetFileName}: Could not apply face preset because it is invalid.");
                        return;
                    }

                    HashSet<string> hashKeys = new HashSet<string>(faceKeys.Concat(faceToggleKeys));

                    foreach (XElement element in faceDataElement.Elements())
                    {
                        System.Xml.IXmlLineInfo info = element;
                        int line = info.HasLineInfo() ? info.LineNumber : -1;
                        string key;

                        if ((key = (string)element.Attribute("name")) == null)
                        {
                            Utility.LogWarning($"{blendSetFileName}: Could not read face blend key at line {line}.");
                            continue;
                        }

                        if (!hashKeys.Contains(key))
                        {
                            Utility.LogWarning($"{blendSetFileName}: Invalid face blend key '{key}' at line {line}.");
                            continue;
                        }

                        if (float.TryParse(element.Value, out float value))
                        {
                            try { SetFaceBlendValue(key, value); }
                            catch { }
                        }
                        else Utility.LogWarning(
                            $"{blendSetFileName}: Could not parse value '{element.Value}' of '{key}' at line {line}"
                        );
                    }
                }
                catch (System.Xml.XmlException e)
                {
                    Utility.LogWarning($"{blendSetFileName}: Face preset data is malformed because {e.Message}");
                    return;
                }
                catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    Utility.LogWarning($"{blendSetFileName}: Could not open face preset because {e.Message}");
                    Constants.InitializeCustomFaceBlends();
                    return;
                }
                catch (Exception e)
                {
                    Utility.LogWarning($"{blendSetFileName}: Could not parse face preset because {e.Message}");
                    return;
                }
            }
            else
            {
                ApplyBackupBlendSet();

                CurrentFaceBlendSet = blendSet;

                BackupBlendSetValues();

                Maid.FaceAnime(blendSet, 0f);
            }

            StopBlink();
            OnUpdateMeido();
        }

        public void SetFaceBlendValue(string hash, float value)
        {
            TMorph morph = Body.Face.morph;
            hash = Utility.GP01FbFaceHash(morph, hash);
            if (morph.Contains(hash))
            {
                if (hash == "nosefook") Maid.boNoseFook = morph.boNoseFook = value > 0f;
                else morph.dicBlendSet[CurrentFaceBlendSet][(int)morph.hash[hash]] = value;
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
            HairGravityControl.Control.OnChangeMekure();
            SkirtGravityControl.Control.OnChangeMekure();
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
            DragPointGravity dragPoint = skirt ? SkirtGravityControl : HairGravityControl;
            if (dragPoint.Valid) dragPoint.Control.transform.localPosition = position;
        }

        private void BackupBlendSetValues()
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
            void CreateCache() => cache.CreateCache(Body.GetBone("Bip01"));
            if (cache == null)
            {
                cache = Maid.gameObject.AddComponent<CacheBoneDataArray>();
                CreateCache();
            }
            if (cache.bone_data?.transform == null)
            {
                Utility.LogDebug("Cache bone_data is null");
                CreateCache();
            }
            return cache;
        }

        private void InitializeGravityControls()
        {
            HairGravityControl = MakeGravityControl(skirt: false);
            SkirtGravityControl = MakeGravityControl(skirt: true);
        }

        private DragPointGravity MakeGravityControl(bool skirt = false)
        {
            DragPointGravity gravityDragpoint = DragPoint.Make<DragPointGravity>(
                PrimitiveType.Cube, Vector3.one * 0.12f
            );
            GravityTransformControl control = DragPointGravity.MakeGravityControl(Maid, skirt);
            gravityDragpoint.Initialize(() => control.transform.position, () => Vector3.zero);
            gravityDragpoint.Set(control.transform);

            gravityDragpoint.gameObject.SetActive(false);

            return gravityDragpoint;
        }

        private void OnUpdateMeido(MeidoUpdateEventArgs args = null)
        {
            UpdateMeido?.Invoke(this, args ?? MeidoUpdateEventArgs.Empty);
        }

        private void OnGravityEvent(object sender, EventArgs args) => OnGravityChange((DragPointGravity)sender);

        private void OnGravityChange(DragPointGravity dragPoint)
        {
            GravityEventArgs args = new GravityEventArgs(
                dragPoint == SkirtGravityControl, dragPoint.MyObject.transform.localPosition
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
                // hair/skirt gravity
                tempWriter.Write(HairGravityActive);
                if (HairGravityActive) tempWriter.WriteVector3(HairGravityControl.Control.transform.localPosition);
                tempWriter.Write(SkirtGravityActive);
                if (SkirtGravityActive) tempWriter.WriteVector3(SkirtGravityControl.Control.transform.localPosition);

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
            IKManager.SetDragPointScale(Maid.transform.localScale.x);
            // pose

            KeyValuePair<bool, bool> muneSetting = new KeyValuePair<bool, bool>(true, true);
            if (mmScene) IKManager.Deserialize(binaryReader);
            else
            {
                int poseBufferLength = binaryReader.ReadInt32();
                byte[] poseBuffer = binaryReader.ReadBytes(poseBufferLength);
                muneSetting = GetCacheBoneData().SetFrameBinary(poseBuffer);
            }

            SetMune(!muneSetting.Key, left: true);
            SetMune(!muneSetting.Value, left: false);

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
            // hair/skirt gravity
            bool hairGravityActive = binaryReader.ReadBoolean();
            if (hairGravityActive)
            {
                HairGravityActive = true;
                ApplyGravity(binaryReader.ReadVector3(), skirt: false);
            }
            bool skirtGravityActive = binaryReader.ReadBoolean();
            if (skirtGravityActive)
            {
                SkirtGravityActive = true;
                ApplyGravity(binaryReader.ReadVector3(), skirt: true);
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

    public readonly struct PoseInfo
    {
        public string PoseGroup { get; }
        public string Pose { get; }
        public bool CustomPose { get; }
        private static readonly PoseInfo defaultPose =
            new PoseInfo(Constants.PoseGroupList[0], Constants.PoseDict[Constants.PoseGroupList[0]][0]);
        public static ref readonly PoseInfo DefaultPose => ref defaultPose;

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
