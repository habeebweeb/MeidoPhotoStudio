using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidFaceWindow : BaseMainWindow
    {
        private MeidoManager meidoManager;
        private MaidFaceSliderPane maidFaceSliderPane;
        private Dropdown faceBlendDropdown;
        private Button facePrevButton;
        private Button faceNextButton;

        public MaidFaceWindow(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.meidoManager.SelectMeido += SelectMeido;

            TabsPane.TabChange += ChangeTab;

            this.maidFaceSliderPane = new MaidFaceSliderPane(this.meidoManager);

            this.faceBlendDropdown = new Dropdown(Translation.GetList("faceBlendPresetsDropdown", Constants.FaceBlendList));
            this.faceBlendDropdown.SelectionChange += (s, a) =>
            {
                string faceBlend = Constants.FaceBlendList[this.faceBlendDropdown.SelectedItemIndex];
                this.meidoManager.ActiveMeido.SetFaceBlend(faceBlend);
                this.UpdateFace();
            };

            this.facePrevButton = new Button("<");
            this.facePrevButton.ControlEvent += (s, a) => this.faceBlendDropdown.Step(-1);

            this.faceNextButton = new Button(">");
            this.faceNextButton.ControlEvent += (s, a) => this.faceBlendDropdown.Step(1);
        }

        ~MaidFaceWindow()
        {
            TabsPane.TabChange -= ChangeTab;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            float arrowButtonSize = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(arrowButtonSize),
                GUILayout.Height(arrowButtonSize)
            };

            float dropdownButtonHeight = arrowButtonSize;
            float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            MaidSwitcherPane.Draw();

            GUI.enabled = this.meidoManager.HasActiveMeido;

            GUILayout.BeginHorizontal();
            this.facePrevButton.Draw(arrowLayoutOptions);
            this.faceBlendDropdown.Draw(dropdownLayoutOptions);
            this.faceNextButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

            this.maidFaceSliderPane.Draw();

            GUILayout.EndScrollView();
        }

        private void UpdateFace()
        {
            if (!this.meidoManager.HasActiveMeido) return;
            if (TabsPane.SelectedTab == Constants.Window.Face)
            {
                this.meidoManager.ActiveMeido.Maid.boMabataki = false;
                this.meidoManager.ActiveMeido.Maid.body0.Face.morph.EyeMabataki = 0f;
                this.maidFaceSliderPane.Update();
            }
        }

        private void SelectMeido(object sender, MeidoChangeEventArgs args)
        {
            UpdateFace();
        }

        private void ChangeTab(object sender, EventArgs args)
        {
            UpdateFace();
        }
    }
}
