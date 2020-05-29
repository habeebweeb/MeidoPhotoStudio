using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidPoseWindow : BaseMainWindow
    {
        private MeidoManager meidoManager;
        private MaidPoseSelectorPane maidPosePane;
        private MaidFaceLookPane maidFaceLookPane;
        private MaidDressingPane maidDressingPane;
        private MaidIKPane maidIKPane;
        private Toggle freeLookToggle;
        public MaidPoseWindow(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.meidoManager.SelectMeido += OnMeidoSelect;

            this.maidPosePane = new MaidPoseSelectorPane(meidoManager);
            this.maidFaceLookPane = new MaidFaceLookPane(meidoManager);
            this.maidFaceLookPane.Enabled = false;

            this.maidDressingPane = new MaidDressingPane(meidoManager);

            this.maidIKPane = new MaidIKPane(meidoManager);

            TabsPane.TabChange += OnTabChange;

            this.freeLookToggle = new Toggle(Translation.Get("freeLook", "freeLookToggle"), false);
            this.freeLookToggle.ControlEvent += (s, a) => SetMaidFreeLook();
        }

        ~MaidPoseWindow()
        {
            TabsPane.TabChange -= OnTabChange;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            MaidSwitcherPane.Draw();
            maidPosePane.Draw();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

            GUILayout.BeginHorizontal();
            GUI.enabled = this.meidoManager.HasActiveMeido;
            freeLookToggle.Draw();
            GUILayout.EndHorizontal();

            maidFaceLookPane.Draw();

            maidDressingPane.Draw();

            MiscGUI.WhiteLine();

            maidIKPane.Draw();

            GUILayout.EndScrollView();
        }

        private void SetMaidFreeLook()
        {
            if (this.updating) return;
            TBody body = this.meidoManager.ActiveMeido.Maid.body0;
            bool isFreeLook = this.freeLookToggle.Value;
            body.trsLookTarget = isFreeLook ? null : GameMain.Instance.MainCamera.transform;
            this.meidoManager.ActiveMeido.IsFreeLook = isFreeLook;
            if (isFreeLook) this.maidFaceLookPane.SetMaidLook();
        }

        private void UpdatePanes()
        {
            if (this.meidoManager.ActiveMeido == null)
            {
                this.updating = true;
                this.freeLookToggle.Value = false;
                this.updating = false;
                return;
            }

            if (TabsPane.SelectedTab == Constants.Window.Pose)
            {
                this.updating = true;
                this.freeLookToggle.Value = this.meidoManager.ActiveMeido?.IsFreeLook ?? false;
                this.updating = false;
                maidPosePane.Update();
                maidFaceLookPane.Update();
                maidDressingPane.Update();
                maidIKPane.Update();
            }
        }

        private void OnMeidoSelect(object sender, MeidoChangeEventArgs args)
        {
            UpdatePanes();
        }

        private void OnTabChange(object sender, EventArgs args)
        {
            UpdatePanes();
        }
    }
}
