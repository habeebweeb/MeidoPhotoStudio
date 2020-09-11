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
        private MpnAttachPropPane mpnAttachPropPane;
        private MaidDressingPane maidDressingPane;
        private GravityControlPane gravityControlPane;
        private CopyPosePane copyPosePane;
        private HandPresetPane handPresetPane;
        private SaveHandPane saveHandPane;
        private MaidIKPane maidIKPane;
        private Toggle freeLookToggle;
        private Toggle savePoseToggle;
        private Toggle saveHandToggle;
        private Button flipButton;
        private bool savePoseMode = false;
        private bool saveHandMode = false;
        private string handPresetHeader;
        private string flipIKHeader;

        public PoseWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
        {
            this.meidoManager = meidoManager;
            this.maidSwitcherPane = maidSwitcherPane;

            this.maidPosePane = AddPane(new MaidPoseSelectorPane(meidoManager));
            this.savePosePane = AddPane(new SavePosePane(meidoManager));

            this.maidFaceLookPane = AddPane(new MaidFaceLookPane(meidoManager));
            this.maidFaceLookPane.Enabled = false;

            this.freeLookToggle = new Toggle(Translation.Get("freeLookPane", "freeLookToggle"), false);
            this.freeLookToggle.ControlEvent += (s, a) => SetMaidFreeLook();

            this.savePoseToggle = new Toggle(Translation.Get("posePane", "saveToggle"));
            this.savePoseToggle.ControlEvent += (s, a) => savePoseMode = !savePoseMode;

            this.mpnAttachPropPane = new MpnAttachPropPane(this.meidoManager);

            this.maidDressingPane = AddPane(new MaidDressingPane(meidoManager));

            this.maidIKPane = AddPane(new MaidIKPane(meidoManager));

            this.gravityControlPane = AddPane(new GravityControlPane(meidoManager));

            this.copyPosePane = AddPane(new CopyPosePane(meidoManager));

            this.saveHandToggle = new Toggle(Translation.Get("handPane", "saveToggle"));
            this.saveHandToggle.ControlEvent += (s, a) => saveHandMode = !saveHandMode;

            this.handPresetPane = AddPane(new HandPresetPane(meidoManager));
            this.saveHandPane = AddPane(new SaveHandPane(meidoManager));

            this.flipButton = new Button(Translation.Get("flipIK", "flipButton"));
            this.flipButton.ControlEvent += (s, a) => this.meidoManager.ActiveMeido.IKManager.Flip();

            this.handPresetHeader = Translation.Get("handPane", "header");
            this.flipIKHeader = Translation.Get("flipIK", "header");
        }

        protected override void ReloadTranslation()
        {
            this.freeLookToggle.Label = Translation.Get("freeLookPane", "freeLookToggle");
            this.savePoseToggle.Label = Translation.Get("posePane", "saveToggle");
            this.saveHandToggle.Label = Translation.Get("handPane", "saveToggle");
            this.flipButton.Label = Translation.Get("flipIK", "flipButton");
            this.handPresetHeader = Translation.Get("handPane", "header");
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

            mpnAttachPropPane.Draw();

            maidDressingPane.Draw();

            MiscGUI.WhiteLine();

            maidIKPane.Draw();

            MiscGUI.WhiteLine();

            this.gravityControlPane.Draw();

            GUI.enabled = this.meidoManager.HasActiveMeido;
            MiscGUI.Header(this.handPresetHeader);
            MiscGUI.WhiteLine();
            saveHandToggle.Draw();
            GUI.enabled = true;

            if (saveHandMode) saveHandPane.Draw();
            else handPresetPane.Draw();

            copyPosePane.Draw();

            GUILayout.BeginHorizontal();
            GUI.enabled = this.meidoManager.HasActiveMeido;
            GUILayout.Label(this.flipIKHeader, GUILayout.ExpandWidth(false));
            flipButton.Draw(GUILayout.ExpandWidth(false));
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void SetMaidFreeLook()
        {
            if (this.updating) return;
            this.meidoManager.ActiveMeido.FreeLook = this.freeLookToggle.Value;
        }

        public override void UpdatePanes()
        {
            if (this.meidoManager.ActiveMeido == null) return;

            if (ActiveWindow)
            {
                this.updating = true;
                this.freeLookToggle.Value = this.meidoManager.ActiveMeido?.FreeLook ?? false;
                this.updating = false;
                base.UpdatePanes();
            }
        }
    }
}
