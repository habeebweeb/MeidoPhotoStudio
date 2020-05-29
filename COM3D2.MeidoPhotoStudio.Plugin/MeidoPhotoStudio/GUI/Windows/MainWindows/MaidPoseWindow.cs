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
            this.freeLookToggle.ControlEvent += (s, a) =>
            {
                TBody body = this.meidoManager.ActiveMeido.Maid.body0;
                body.trsLookTarget = this.freeLookToggle.Value ? null : GameMain.Instance.MainCamera.transform;
                this.maidFaceLookPane.Enabled = this.freeLookToggle.Value;
                if (this.freeLookToggle.Value) this.maidFaceLookPane.SetMaidLook();
            };
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

        private void UpdatePanes()
        {
            if (!this.meidoManager.HasActiveMeido) return;

            if (TabsPane.SelectedTab == Constants.Window.Pose)
            {
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
