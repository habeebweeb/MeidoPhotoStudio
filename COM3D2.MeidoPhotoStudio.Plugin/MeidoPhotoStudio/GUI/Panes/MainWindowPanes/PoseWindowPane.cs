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
        private CopyPosePane copyPosePane;
        private HandPresetPane handPresetPane;
        private SaveHandPane saveHandPane;
        private MaidIKPane maidIKPane;
        private Toggle freeLookToggle;
        private Toggle savePoseToggle;
        private Toggle saveHandToggle;
        private bool savePoseMode = false;
        private bool saveHandMode = false;

        public PoseWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
        {
            this.meidoManager = meidoManager;
            this.maidSwitcherPane = maidSwitcherPane;

            this.maidPosePane = AddPane(new MaidPoseSelectorPane(meidoManager));
            this.savePosePane = AddPane(new SavePosePane(meidoManager));

            this.maidFaceLookPane = AddPane(new MaidFaceLookPane(meidoManager));
            this.maidFaceLookPane.Enabled = false;

            this.freeLookToggle = new Toggle(Translation.Get("freeLook", "freeLookToggle"), false);
            this.freeLookToggle.ControlEvent += (s, a) => SetMaidFreeLook();

            this.savePoseToggle = new Toggle(Translation.Get("posePane", "saveToggle"));
            this.savePoseToggle.ControlEvent += (s, a) => savePoseMode = !savePoseMode;

            this.maidDressingPane = AddPane(new MaidDressingPane(meidoManager));

            this.maidIKPane = AddPane(new MaidIKPane(meidoManager));

            this.copyPosePane = AddPane(new CopyPosePane(meidoManager));

            this.saveHandToggle = new Toggle(Translation.Get("handPane", "saveToggle"));
            this.saveHandToggle.ControlEvent += (s, a) => saveHandMode = !saveHandMode;

            this.handPresetPane = AddPane(new HandPresetPane(meidoManager));
            this.saveHandPane = AddPane(new SaveHandPane(meidoManager));
        }

        protected override void ReloadTranslation()
        {
            this.freeLookToggle.Label = Translation.Get("freeLook", "freeLookToggle");
            this.savePoseToggle.Label = Translation.Get("posePane", "saveToggle");
            this.saveHandToggle.Label = Translation.Get("handPane", "saveToggle");
        }

        public override void Draw()
        {
            this.maidSwitcherPane.Draw();
            maidPosePane.Draw();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

            GUI.enabled = this.meidoManager.HasActiveMeido;
            GUILayout.BeginHorizontal();
            freeLookToggle.Draw();
            savePoseToggle.Draw();
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            if (savePoseMode) savePosePane.Draw();
            else maidFaceLookPane.Draw();

            maidDressingPane.Draw();

            MiscGUI.WhiteLine();

            maidIKPane.Draw();

            GUI.enabled = this.meidoManager.HasActiveMeido;
            saveHandToggle.Draw();
            GUI.enabled = true;

            if (saveHandMode) saveHandPane.Draw();
            else handPresetPane.Draw();

            copyPosePane.Draw();

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
                base.UpdatePanes();
            }
        }

        private void UpdateMeido(object sender, EventArgs args)
        {
            this.UpdatePanes();
        }
    }
}
