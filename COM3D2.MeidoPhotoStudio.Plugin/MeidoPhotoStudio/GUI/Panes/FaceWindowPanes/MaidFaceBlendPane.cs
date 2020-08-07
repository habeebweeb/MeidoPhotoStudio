using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidFaceBlendPane : BasePane
    {
        private MeidoManager meidoManager;
        private Dropdown faceBlendDropdown;
        private Button facePrevButton;
        private Button faceNextButton;

        public MaidFaceBlendPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            this.faceBlendDropdown = new Dropdown(
                Translation.GetArray("faceBlendPresetsDropdown", Constants.FaceBlendList)
            );
            this.faceBlendDropdown.SelectionChange += (s, a) =>
            {
                if (updating) return;
                string faceBlend = Constants.FaceBlendList[this.faceBlendDropdown.SelectedItemIndex];
                this.meidoManager.ActiveMeido.SetFaceBlendSet(faceBlend);
            };

            this.facePrevButton = new Button("<");
            this.facePrevButton.ControlEvent += (s, a) => this.faceBlendDropdown.Step(-1);

            this.faceNextButton = new Button(">");
            this.faceNextButton.ControlEvent += (s, a) => this.faceBlendDropdown.Step(1);
        }

        protected override void ReloadTranslation()
        {
            this.updating = true;
            faceBlendDropdown.SetDropdownItems(
                Translation.GetArray("faceBlendPresetsDropdown", Constants.FaceBlendList)
            );
            this.updating = false;
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

            GUI.enabled = this.meidoManager.HasActiveMeido;

            GUILayout.BeginHorizontal();
            this.facePrevButton.Draw(arrowLayoutOptions);
            this.faceBlendDropdown.Draw(dropdownLayoutOptions);
            this.faceNextButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        public override void UpdatePane()
        {
            this.updating = true;
            int faceBlendSetIndex = Constants.FaceBlendList.FindIndex(
                blend => blend == this.meidoManager.ActiveMeido.FaceBlendSet
            );
            this.faceBlendDropdown.SelectedItemIndex = Mathf.Clamp(faceBlendSetIndex, 0, Constants.FaceBlendList.Count);
            this.updating = false;
        }
    }
}
