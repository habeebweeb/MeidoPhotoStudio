using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class PoseWindowPane : BaseWindowPane
    {
        private MeidoManager meidoManager;
        private MaidPoseSelectorPane maidPosePane;
        private SavePosePane savePosePane;
        private MaidSwitcherPane maidSwitcherPane;
        private MaidFaceLookPane maidFaceLookPane;
        private MaidDressingPane maidDressingPane;
        private MaidIKPane maidIKPane;
        private Toggle freeLookToggle;
        private Toggle savePoseToggle;

        private bool savePoseMode = false;

        public PoseWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
        {
            this.meidoManager = meidoManager;
            this.maidSwitcherPane = maidSwitcherPane;

            this.savePosePane = new SavePosePane(meidoManager);
            this.maidPosePane = new MaidPoseSelectorPane(meidoManager);
            this.maidFaceLookPane = new MaidFaceLookPane(meidoManager);
            this.maidFaceLookPane.Enabled = false;

            this.maidDressingPane = new MaidDressingPane(meidoManager);

            this.maidIKPane = new MaidIKPane(meidoManager);

            this.freeLookToggle = new Toggle(Translation.Get("freeLook", "freeLookToggle"), false);
            this.freeLookToggle.ControlEvent += (s, a) => SetMaidFreeLook();

            this.savePoseToggle = new Toggle(Translation.Get("posePane", "saveToggle"));
            this.savePoseToggle.ControlEvent += (s, a) => savePoseMode = !savePoseMode;
        }

        protected override void ReloadTranslation()
        {
            this.freeLookToggle.Label = Translation.Get("freeLook", "freeLookToggle");
            this.savePoseToggle.Label = Translation.Get("posePane", "saveToggle");
        }

        public override void Draw()
        {
            this.maidSwitcherPane.Draw();
            maidPosePane.Draw();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

            GUILayout.BeginHorizontal();
            GUI.enabled = this.meidoManager.HasActiveMeido;
            freeLookToggle.Draw();
            savePoseToggle.Draw();
            GUILayout.EndHorizontal();

            if (savePoseMode) savePosePane.Draw();
            else maidFaceLookPane.Draw();

            maidDressingPane.Draw();

            MiscGUI.WhiteLine();

            maidIKPane.Draw();

            GUILayout.EndScrollView();
        }

        private void SetMaidFreeLook()
        {
            if (this.updating) return;
            this.meidoManager.ActiveMeido.IsFreeLook = this.freeLookToggle.Value;
        }

        public override void UpdatePanes()
        {
            if (this.meidoManager.ActiveMeido == null) return;

            if (ActiveWindow)
            {
                this.updating = true;
                this.freeLookToggle.Value = this.meidoManager.ActiveMeido?.IsFreeLook ?? false;
                this.updating = false;
                maidPosePane.UpdatePane();
                maidFaceLookPane.UpdatePane();
                maidDressingPane.UpdatePane();
                maidIKPane.UpdatePane();
            }
        }

        private void UpdateMeido(object sender, EventArgs args)
        {
            this.UpdatePanes();
        }
    }
}
