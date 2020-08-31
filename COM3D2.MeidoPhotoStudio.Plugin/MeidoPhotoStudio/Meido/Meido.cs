using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using static TBody;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class Meido : ISerializable
    {
        private const int maxMaids = 12;
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
        private bool initialized;
        public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;
        public int StockNo { get; }
        public Maid Maid { get; private set; }
        public TBody Body => Maid.body0;
        public MeidoDragPointManager IKManager { get; private set; }
        public Texture2D Portrait { get; private set; }
        public PoseInfo CachedPose { get; private set; } = DefaultPose;
        public string FaceBlendSet { get; private set; } = defaultFaceBlendSet;
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
                if (this.freeLook == value) return;
                this.freeLook = value;
                Body.trsLookTarget = this.freeLook ? null : GameMain.Instance.MainCamera.transform;
                OnUpdateMeido();
            }
        }
        public bool Stop
        {
            get
            {
                if (!Body.isLoadedBody) return true;
                else return !Maid.GetAnimation().isPlaying;
            }
            set
            {
                if (!Body.isLoadedBody || value == Stop) return;
                else
                {
                    if (value) Maid.GetAnimation().Stop();
                    else this.SetPose(this.CachedPose.Pose);
                    OnUpdateMeido();
                }
            }
        }
        public bool IK
        {
            get => IKManager.Active;
            set
            {
                if (value == IKManager.Active) return;
                else IKManager.Active = value;
            }
        }
        public bool Bone
        {
            get => IKManager.IsBone;
            set
            {
                if (value == Bone) return;
                else IKManager.IsBone = value;
                OnUpdateMeido();
            }
        }
        public float[] BlendValuesBackup { get; private set; }
        public float[] BlendValues { get; private set; }
        public Quaternion DefaultEyeRotL { get; private set; }
        public Quaternion DefaultEyeRotR { get; private set; }

        public Meido(int stockMaidIndex)
        {
            this.StockNo = stockMaidIndex;
            this.Maid = GameMain.Instance.CharacterMgr.GetStockMaid(stockMaidIndex);
            this.Portrait = Maid.GetThumIcon();
            Maid.boAllProcPropBUSY = false;
            IKManager = new MeidoDragPointManager(this);
            IKManager.SelectMaid += (s, args) => OnUpdateMeido((MeidoUpdateEventArgs)args);
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
                Body.jbMuneL.enabled = true;
                Body.jbMuneR.enabled = true;
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
            Unload();

            Maid.SetPos(Vector3.zero);
            Maid.SetRot(Vector3.zero);
            Maid.SetPosOffset(Vector3.zero);
            Body.transform.localScale = Vector3.one;

            Maid.DelPrefabAll();
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

        public void SetFaceBlendSet(string blendSet)
        {
            FaceBlendSet = blendSet;
            Maid.boMabataki = false;
            TMorph morph = Body.Face.morph;
            morph.EyeMabataki = 0f;
            morph.MulBlendValues(blendSet, 1f);
            morph.FixBlendValues_Face();
            OnUpdateMeido();
        }

        public void SetFaceBlendValue(string hash, float value)
        {
            TMorph morph = Body.Face.morph;
            if (hash == "nosefook")
            {
                morph.boNoseFook = value > 0f;
                Maid.boNoseFook = morph.boNoseFook;
            }
            else
            {
                bool gp01FBFace = morph.bodyskin.PartsVersion >= 120;
                float[] blendValues = hash.StartsWith("eyeclose") && !(gp01FBFace && (hash == "eyeclose3"))
                    ? this.BlendValuesBackup
                    : this.BlendValues;

                hash = Utility.GP01FbFaceHash(morph, hash);

                try
                {
                    blendValues[(int)morph.hash[hash]] = value;
                }
                catch { }
            }
            Maid.boMabataki = false;
            morph.EyeMabataki = 0f;
            morph.FixBlendValues_Face();
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
        }

        private CacheBoneDataArray GetCacheBoneData()
        {
            CacheBoneDataArray cache = this.Maid.gameObject.GetComponent<CacheBoneDataArray>();
            if (cache == null)
            {
                cache = this.Maid.gameObject.AddComponent<CacheBoneDataArray>();
                cache.CreateCache(this.Maid.body0.GetBone("Bip01"));
            }
            return cache;
        }

        private void OnBodyLoad()
        {
            if (!initialized)
            {
                TMorph faceMorph = Body.Face.morph;
                BlendValuesBackup = Utility.GetFieldValue<TMorph, float[]>(faceMorph, "BlendValuesBackup");
                BlendValues = Utility.GetFieldValue<TMorph, float[]>(faceMorph, "BlendValues");
                DefaultEyeRotL = Body.quaDefEyeL;
                DefaultEyeRotR = Body.quaDefEyeR;
                initialized = true;
            }

            IKManager.Initialize();

            IK = true;
            Stop = false;
            Bone = false;
            Loading = false;
        }

        public void OnUpdateMeido(MeidoUpdateEventArgs args = null)
        {
            this.UpdateMeido?.Invoke(this, args ?? MeidoUpdateEventArgs.Empty);
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
                tempWriter.WriteVector3(Body.offsetLookTarget);
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

                binaryWriter.Write(memoryStream.Length);
                binaryWriter.Write(memoryStream.ToArray());
            }
        }

        private void SerializeFace(BinaryWriter binaryWriter)
        {
            binaryWriter.Write("MPS_FACE");
            TMorph morph = Maid.body0.Face.morph;
            bool gp01FBFace = morph.bodyskin.PartsVersion >= 120;
            foreach (string hash in faceKeys)
            {
                float[] blendValues = hash.StartsWith("eyeclose") && !(gp01FBFace && (hash == "eyeclose3"))
                    ? this.BlendValuesBackup
                    : this.BlendValues;

                string faceKey = Utility.GP01FbFaceHash(morph, hash);
                try
                {
                    float value = blendValues[(int)morph.hash[faceKey]];
                    binaryWriter.Write(hash);
                    binaryWriter.Write(value);
                }
                catch { }
            }

            foreach (string hash in faceToggleKeys)
            {
                bool value = this.BlendValues[(int)morph.hash[hash]] > 0f;
                if (hash == "nosefook") value = morph.boNoseFook;

                binaryWriter.Write(hash);
                binaryWriter.Write(value);
            }
            binaryWriter.Write("END_FACE");
        }

        public void Deserialize(BinaryReader binaryReader) => Deserialize(binaryReader, false);

        public void Deserialize(BinaryReader binaryReader, bool mmScene)
        {
            Maid.GetAnimation().Stop();
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
            CachedPose = PoseInfo.Deserialize(binaryReader);
            // eye direction
            Body.quaDefEyeL = DefaultEyeRotL * binaryReader.ReadQuaternion();
            Body.quaDefEyeR = DefaultEyeRotR * binaryReader.ReadQuaternion();
            // free look
            FreeLook = binaryReader.ReadBoolean();
            Vector3 lookTarget = binaryReader.ReadVector3();
            if (FreeLook) Body.offsetLookTarget = lookTarget;
            // face
            DeserializeFace(binaryReader);
            // body visible
            SetBodyMask(binaryReader.ReadBoolean());
            // clothing
            foreach (SlotID clothingSlot in MaidDressingPane.clothingSlots)
            {
                bool value = binaryReader.ReadBoolean();
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
            if (CurlingFront != curlingFront) SetCurling(Curl.front, curlingFront);
            if (CurlingBack != curlingBack) SetCurling(Curl.back, curlingBack);
            SetCurling(Curl.shift, binaryReader.ReadBoolean());
            // OnUpdateMeido();
        }

        private void DeserializeFace(BinaryReader binaryReader)
        {
            binaryReader.ReadString(); // read face header
            TMorph morph = Maid.body0.Face.morph;
            bool gp01FBFace = morph.bodyskin.PartsVersion >= 120;
            HashSet<string> faceKeys = new HashSet<string>(Meido.faceKeys);
            string header;
            while ((header = binaryReader.ReadString()) != "END_FACE")
            {
                if (faceKeys.Contains(header))
                {
                    float[] blendValues = header.StartsWith("eyeclose") && !(gp01FBFace && (header == "eyeclose3"))
                        ? this.BlendValuesBackup
                        : this.BlendValues;
                    string hash = Utility.GP01FbFaceHash(morph, header);
                    try
                    {
                        float value = binaryReader.ReadSingle();
                        blendValues[(int)morph.hash[hash]] = value;
                    }
                    catch { }
                }
                else
                {
                    bool value = binaryReader.ReadBoolean();
                    if (header == "nosefook") this.Maid.boNoseFook = value;
                    else this.BlendValues[(int)morph.hash[header]] = value ? 1f : 0f;
                }
            }
            Maid.boMabataki = false;
            morph.EyeMabataki = 0f;
            morph.FixBlendValues_Face();
        }
    }

    public struct PoseInfo
    {
        public string PoseGroup { get; }
        public string Pose { get; }
        public bool CustomPose { get; }
        public PoseInfo(string poseGroup, string pose, bool customPose = false)
        {
            this.PoseGroup = poseGroup;
            this.Pose = pose;
            this.CustomPose = customPose;
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
