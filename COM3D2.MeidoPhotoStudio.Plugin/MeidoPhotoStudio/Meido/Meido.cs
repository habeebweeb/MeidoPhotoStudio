using System;
using System.Collections;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class Meido
    {
        private const int MAX_MAIDS = 12;
        private static CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
        public readonly int stockNo;
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
        public static readonly PoseInfo defaultPose
            = new PoseInfo(Constants.PoseGroupList[0], Constants.PoseDict[Constants.PoseGroupList[0]][0]);
        public static readonly string defaultFaceBlendSet = "通常";
        private MeidoDragPointManager dragPointManager;
        private bool initialized = false;
        public Maid Maid { get; private set; }
        public Texture2D Image { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string NameJP => $"{LastName}\n{FirstName}";
        public string NameEN => $"{FirstName}\n{LastName}";
        public int ActiveSlot { get; private set; }
        public event EventHandler<MeidoUpdateEventArgs> UpdateMeido;
        public event EventHandler BodyLoad;
        private bool isLoading = false;
        public float[] BlendValuesBackup { get; private set; }
        public float[] BlendValues { get; private set; }
        public bool IsIK
        {
            get => dragPointManager?.Active ?? false;
            set
            {
                if (dragPointManager == null || value == dragPointManager.Active) return;
                else dragPointManager.Active = value;
            }
        }
        public bool IsStop
        {
            get
            {
                if (!Maid.body0.isLoadedBody) return true;
                else return !Maid.GetAnimation().isPlaying;
            }
            set
            {
                if (!Maid.body0.isLoadedBody || value == !Maid.GetAnimation().isPlaying) return;
                else
                {
                    if (value) Maid.GetAnimation().Stop();
                    else this.SetPose(this.CachedPose.Pose);
                    OnUpdateMeido();
                }
            }
        }
        // private bool isBone = false;
        public bool IsBone
        {
            get => dragPointManager?.IsBone ?? false;
            set
            {
                if (dragPointManager == null || value == dragPointManager.IsBone) return;
                else dragPointManager.IsBone = value;
                OnUpdateMeido();
            }
        }
        private bool isFreeLook;
        public bool IsFreeLook
        {
            get => isFreeLook;
            set
            {
                if (this.isFreeLook == value) return;
                this.isFreeLook = value;
                Maid.body0.trsLookTarget = this.isFreeLook ? null : GameMain.Instance.MainCamera.transform;
                OnUpdateMeido();
            }
        }
        public PoseInfo CachedPose { get; private set; }
        public string FaceBlendSet { get; private set; } = defaultFaceBlendSet;

        public Meido(int stockMaidIndex)
        {
            this.Maid = characterMgr.GetStockMaid(stockMaidIndex);
            this.stockNo = stockMaidIndex;
            this.Image = Maid.GetThumIcon();
            this.FirstName = Maid.status.firstName;
            this.LastName = Maid.status.lastName;
            this.CachedPose = defaultPose;
            // I don't know why I put this here. Must've fixed something with proc loading
            Maid.boAllProcPropBUSY = false;
        }

        public void Update()
        {
            if (isLoading)
            {
                if (!Maid.IsBusy)
                {
                    isLoading = false;
                    OnBodyLoad();
                }
            }
        }

        public void Flip()
        {
            if (this.dragPointManager == null) return;
            IsStop = true;
            this.dragPointManager.Flip();
        }

        public byte[] SerializePose()
        {
            CacheBoneDataArray cache = this.Maid.gameObject.GetComponent<CacheBoneDataArray>();

            if (cache == null)
            {
                cache = this.Maid.gameObject.AddComponent<CacheBoneDataArray>();
                cache.CreateCache(this.Maid.body0.GetBone("Bip01"));
            }

            return cache.GetAnmBinary(true, true);
        }

        public void SetHandPreset(string filename, bool right)
        {
            if (this.dragPointManager == null) return;

            XDocument handDocument = XDocument.Load(filename);
            XElement handElement = handDocument.Element("FingerData");
            if (handElement.IsEmpty || handElement.Element("GameVersion").IsEmpty
                || handElement.Element("RightData").IsEmpty || handElement.Element("BinaryData").IsEmpty)
            {
                return;
            }

            IsStop = true;

            bool rightData = bool.Parse(handElement.Element("RightData").Value);
            string base64Data = handElement.Element("BinaryData").Value;

            byte[] handData = Convert.FromBase64String(base64Data);

            this.dragPointManager.DeserializeHand(handData, right, rightData != right);
        }

        public byte[] SerializeHand(bool right) => this.dragPointManager?.SerializeHand(right);

        public Maid Load(int activeSlot, int maidSlot)
        {
            isLoading = true;
            this.ActiveSlot = activeSlot;

            Maid.Visible = true;

            if (!Maid.body0.isLoadedBody)
            {
                if (maidSlot >= MAX_MAIDS)
                {
                    Maid.DutPropAll();
                    Maid.AllProcPropSeqStart();
                }
                else
                {
                    GameMain.Instance.CharacterMgr.Activate(maidSlot, maidSlot, false, false);
                    GameMain.Instance.CharacterMgr.CharaVisible(maidSlot, true, false);
                }
            }
            else
            {
                SetPose(defaultPose);
            }

            dragPointManager = new MeidoDragPointManager(this);
            dragPointManager.SelectMaid += OnMeidoSelect;

            this.IsFreeLook = false;
            Maid.body0.boHeadToCam = true;
            Maid.body0.boEyeToCam = true;
            Maid.body0.SetBoneHitHeightY(-1000f);

            return Maid;
        }

        public void Unload()
        {
            if (Maid.body0.isLoadedBody)
            {
                Maid.body0.MuneYureL(1f);
                Maid.body0.MuneYureR(1f);
                Maid.body0.jbMuneL.enabled = true;
                Maid.body0.jbMuneR.enabled = true;
            }

            Maid.body0.SetMaskMode(TBody.MaskMode.None);

            Maid.body0.trsLookTarget = GameMain.Instance.MainCamera.transform;

            Maid.Visible = false;

            if (dragPointManager != null)
            {
                dragPointManager.Destroy();
                dragPointManager.SelectMaid -= OnMeidoSelect;
                dragPointManager = null;
            }
        }

        public void Deactivate()
        {
            Unload();

            Maid.SetPos(Vector3.zero);
            Maid.SetRot(Vector3.zero);
            Maid.SetPosOffset(Vector3.zero);
            Maid.body0.SetBoneHitHeightY(0f);

            Maid.DelPrefabAll();

            Maid.ActiveSlotNo = -1;
        }

        public void SetPose(PoseInfo poseInfo)
        {
            this.CachedPose = poseInfo;
            SetPose(poseInfo.Pose);
        }

        public void SetPose(string pose)
        {
            if (!Maid.body0.isLoadedBody) return;
            if (pose.StartsWith(Constants.customPosePath))
            {
                SetPoseCustom(pose);
                return;
            }

            string[] poseComponents = pose.Split(',');
            pose = poseComponents[0] + ".anm";

            Maid.CrossFade(pose, false, true, false, 0f);
            Maid.SetAutoTwistAll(true);
            Maid.GetAnimation().Play();

            if (poseComponents.Length > 1)
            {
                Maid.GetAnimation()[pose].time = float.Parse(poseComponents[1]);
                Maid.GetAnimation()[pose].speed = 0f;
            }

            SetMune();
        }

        public void CopyPose(Meido fromMeido)
        {
            byte[] poseBinary = fromMeido.SerializePose();
            string tag = $"copy_{fromMeido.Maid.status.guid}";
            Maid.body0.CrossFade(tag, poseBinary, false, true, false, 0f);
            Maid.SetAutoTwistAll(true);
            Maid.transform.rotation = fromMeido.Maid.transform.rotation;
            SetMune();
        }

        public void SetPoseCustom(string path)
        {
            if (!Maid.body0.isLoadedBody) return;
            byte[] bytes = File.ReadAllBytes(path);
            string hash = Path.GetFileName(path).GetHashCode().ToString();
            Maid.body0.CrossFade(hash, bytes, false, true, false, 0f);
            Maid.SetAutoTwistAll(true);
            SetMune();
        }

        public void SetMune(bool drag = false)
        {
            bool isMomiOrPaizuri = CachedPose.Pose.Contains("_momi") || CachedPose.Pose.Contains("paizuri_");
            float onL = (drag || isMomiOrPaizuri) ? 0f : 1f;
            Maid.body0.MuneYureL(onL);
            Maid.body0.MuneYureR(onL);
            Maid.body0.jbMuneL.enabled = !drag;
            Maid.body0.jbMuneR.enabled = !drag;
        }

        public void SetFaceBlendSet(string blendSet)
        {
            FaceBlendSet = blendSet;
            Maid.boMabataki = false;
            TMorph morph = Maid.body0.Face.morph;
            morph.EyeMabataki = 0f;
            morph.MulBlendValues(blendSet, 1f);
            morph.FixBlendValues_Face();
            OnUpdateMeido();
        }

        public void SetFaceBlendValue(string hash, float value)
        {
            TMorph morph = Maid.body0.Face.morph;
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

        public void SetMaskMode(TBody.MaskMode maskMode)
        {
            TBody body = Maid.body0;
            bool invisibleBody = !body.GetMask(TBody.SlotID.body);
            body.SetMaskMode(maskMode);
            if (invisibleBody) SetBodyMask(false);
        }

        public void SetBodyMask(bool enabled)
        {
            TBody body = Maid.body0;
            Hashtable table = Utility.GetFieldValue<TBody, Hashtable>(body, "m_hFoceHide");
            foreach (TBody.SlotID bodySlot in MaidDressingPane.bodySlots)
            {
                table[bodySlot] = enabled;
            }
            if (body.goSlot[19].m_strModelFileName.Contains("melala_body"))
            {
                table[TBody.SlotID.accHana] = enabled;
            }
            body.FixMaskFlag();
            body.FixVisibleFlag(false);
        }

        public Transform GetBoneTransform(AttachPoint point) => this.dragPointManager?.GetAttachPointTransform(point);

        private void OnBodyLoad()
        {
            BodyLoad?.Invoke(this, EventArgs.Empty);

            if (!initialized)
            {
                TMorph morph = Maid.body0.Face.morph;
                this.BlendValuesBackup = Utility.GetFieldValue<TMorph, float[]>(morph, "BlendValuesBackup");
                this.BlendValues = Utility.GetFieldValue<TMorph, float[]>(morph, "BlendValues");

                initialized = true;
            }

            this.IsIK = true;
            this.IsStop = false;
            this.IsBone = false;
        }

        private void OnUpdateMeido(MeidoUpdateEventArgs args = null)
        {
            this.UpdateMeido?.Invoke(this, args ?? MeidoUpdateEventArgs.Empty);
        }

        private void OnMeidoSelect(object sender, MeidoUpdateEventArgs args)
        {
            UpdateMeido?.Invoke(this, args);
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
    }
}
