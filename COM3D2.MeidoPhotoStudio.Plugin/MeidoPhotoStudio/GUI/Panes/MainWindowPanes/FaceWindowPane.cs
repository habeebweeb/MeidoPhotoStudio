using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class FaceWindowPane : BaseWindowPane
    {
        private MeidoManager meidoManager;
        private MaidFaceSliderPane maidFaceSliderPane;
        private MaidSwitcherPane maidSwitcherPane;
        private Dropdown faceBlendDropdown;
        private Button facePrevButton;
        private Button faceNextButton;

        public FaceWindowPane(MeidoManager meidoManager, MaidSwitcherPane maidSwitcherPane)
        {
            this.meidoManager = meidoManager;
            // this.meidoManager.UpdateMeido += UpdateMeido;

            this.maidSwitcherPane = maidSwitcherPane;

            this.maidFaceSliderPane = new MaidFaceSliderPane(this.meidoManager);

            this.faceBlendDropdown = new Dropdown(
                Translation.GetArray("faceBlendPresetsDropdown", Constants.FaceBlendList)
            );
            this.faceBlendDropdown.SelectionChange += (s, a) =>
            {
                if (updating) return;
                string faceBlend = Constants.FaceBlendList[this.faceBlendDropdown.SelectedItemIndex];
                this.meidoManager.ActiveMeido.SetFaceBlend(faceBlend);
                this.UpdatePanes();
            };

            this.facePrevButton = new Button("<");
            this.facePrevButton.ControlEvent += (s, a) => this.faceBlendDropdown.Step(-1);

            this.faceNextButton = new Button(">");
            this.faceNextButton.ControlEvent += (s, a) => this.faceBlendDropdown.Step(1);
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            faceBlendDropdown.SetDropdownItems(
                Translation.GetArray("faceBlendPresetsDropdown", Constants.FaceBlendList)
            );
            updating = false;
        }

        public override void Draw()
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

            this.maidSwitcherPane.Draw();

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

        public override void UpdatePanes()
        {
            if (!this.meidoManager.HasActiveMeido) return;
            if (ActiveWindow)
            {
                this.meidoManager.ActiveMeido.Maid.boMabataki = false;
                this.meidoManager.ActiveMeido.Maid.body0.Face.morph.EyeMabataki = 0f;
                this.maidFaceSliderPane.UpdatePane();
            }
        }

        private void UpdateMeido(object sender, EventArgs args)
        {
            UpdatePanes();
        }
    }
}
