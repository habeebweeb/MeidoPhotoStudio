using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Meido
    {
        private static CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
        public readonly int stockNo;
        public Maid Maid { get; private set; }
        public Texture2D Image { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string NameJP => $"{LastName}\n{FirstName}";
        public string NameEN => $"{FirstName}\n{LastName}";
        public int ActiveSlot { get; private set; }
        private DragPointManager dragPointManager;
        public event EventHandler<MeidoChangeEventArgs> SelectMeido;
        public event EventHandler BodyLoad;
        private bool isLoading = false;
        public bool Visible
        {
            get => Maid.Visible;
            set => Maid.Visible = value;
        }

        public Meido(int stockMaidIndex)
        {
            this.Maid = characterMgr.GetStockMaid(stockMaidIndex);
            this.stockNo = stockMaidIndex;
            this.Image = Maid.GetThumIcon();
            this.FirstName = Maid.status.firstName;
            this.LastName = Maid.status.lastName;
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
                return;
            }
            dragPointManager.Update();
        }

        public Maid Load(int activeSlot)
        {
            isLoading = true;
            this.ActiveSlot = activeSlot;

            Maid.Visible = true;

            if (!Maid.body0.isLoadedBody)
            {
                Maid.DutPropAll();
                Maid.AllProcPropSeqStart();
            }
            else
            {
                SetPose("pose_taiki_f");
            }

            if (dragPointManager == null)
            {
                dragPointManager = new DragPointManager(this);
                dragPointManager.SelectMaid += (sender, meidoChangeArgs) => OnMeidoSelect(meidoChangeArgs);
            }
            else
            {
                dragPointManager.Activate();
            }

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

            dragPointManager?.Deactivate();
        }

        public void Deactivate()
        {
            Unload();
            dragPointManager?.Destroy();
            Maid.SetPos(Vector3.zero);
            Maid.SetRot(Vector3.zero);
            Maid.SetPosOffset(Vector3.zero);
            Maid.body0?.SetBoneHitHeightY(0f);

            Maid.Visible = false;
            Maid.ActiveSlotNo = -1;
            Maid.DelPrefabAll();
        }

        public void SetPose(string pose)
        {
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

            if (pose.Contains("_momi") || pose.Contains("paizuri_"))
            {
                Maid.body0.MuneYureL(0f);
                Maid.body0.MuneYureR(0f);
            }
            else
            {
                Maid.body0.MuneYureL(1f);
                Maid.body0.MuneYureR(1f);
            }
        }

        public void SetPoseCustom(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            string hash = Path.GetFileName(path).GetHashCode().ToString();
            Maid.body0.CrossFade(hash, bytes, false, true, false, 0f);
            Maid.SetAutoTwistAll(true);
        }

        public void SetFaceBlend(string blendValue)
        {
            Maid.boMabataki = false;
            TMorph morph = Maid.body0.Face.morph;
            morph.EyeMabataki = 0f;
            morph.MulBlendValues(blendValue, 1f);
            morph.FixBlendValues_Face();
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
                float[] blendValues = hash.StartsWith("eyeclose")
                    ? Utility.GetFieldValue<TMorph, float[]>(morph, "BlendValuesBackup")
                    : Utility.GetFieldValue<TMorph, float[]>(morph, "BlendValues");

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

        private void OnBodyLoad()
        {
            BodyLoad?.Invoke(this, EventArgs.Empty);
        }

        private void OnMeidoSelect(MeidoChangeEventArgs args)
        {
            SelectMeido?.Invoke(this, args);
        }
    }
}
