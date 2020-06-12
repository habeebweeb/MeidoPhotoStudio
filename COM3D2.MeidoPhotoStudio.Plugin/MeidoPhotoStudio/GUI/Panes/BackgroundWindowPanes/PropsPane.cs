using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    class PropsPane : BasePane
    {
        private EnvironmentManager environmentManager;
        private PropManager propManager;
        private Dropdown otherDoguDropdown;
        private Dropdown doguDropdown;
        private Button addDoguButton;
        private Button addOtherDoguButton;
        private Button nextDoguButton;
        private Button prevDoguButton;
        private Button nextOtherDoguButton;
        private Button prevOtherDoguButton;

        public PropsPane(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
            this.propManager = this.environmentManager.PropManager;

            this.doguDropdown = new Dropdown(Translation.GetArray("props1Dropdown", Constants.DoguList));

            this.otherDoguDropdown = new Dropdown(Translation.GetArray("props2Dropdown", Constants.OtherDoguList));

            this.addOtherDoguButton = new Button("+");
            this.addOtherDoguButton.ControlEvent += (s, a) =>
            {
                string assetName = Constants.OtherDoguList[this.otherDoguDropdown.SelectedItemIndex];
                this.propManager.SpawnObject(assetName);
            };

            this.addDoguButton = new Button("+");
            this.addDoguButton.ControlEvent += (s, a) =>
            {
                string assetName = Constants.DoguList[this.doguDropdown.SelectedItemIndex];
                this.propManager.SpawnObject(assetName);
            };

            this.nextDoguButton = new Button(">");
            this.nextDoguButton.ControlEvent += (s, a) => this.doguDropdown.Step(1);


            this.prevDoguButton = new Button("<");
            this.prevDoguButton.ControlEvent += (s, a) => this.doguDropdown.Step(-1);

            this.nextOtherDoguButton = new Button(">");
            this.nextOtherDoguButton.ControlEvent += (s, a) => this.otherDoguDropdown.Step(1);

            this.prevOtherDoguButton = new Button("<");
            this.prevOtherDoguButton.ControlEvent += (s, a) => this.otherDoguDropdown.Step(-1);
        }

        protected override void ReloadTranslation()
        {
            this.doguDropdown.SetDropdownItems(Translation.GetArray("props1Dropdown", Constants.DoguList));
            this.otherDoguDropdown.SetDropdownItems(Translation.GetArray("props2Dropdown", Constants.OtherDoguList));
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            float arrowButtonSize = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(arrowButtonSize),
                GUILayout.Height(arrowButtonSize)
            };

            float dropdownButtonHeight = arrowButtonSize;
            float dropdownButtonWidth = 120f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            MiscGUI.Header("Props 1");
            GUILayout.BeginHorizontal();
            this.doguDropdown.Draw(dropdownLayoutOptions);
            this.prevDoguButton.Draw(arrowLayoutOptions);
            this.nextDoguButton.Draw(arrowLayoutOptions);
            this.addDoguButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            MiscGUI.Header("Props 2");
            GUILayout.BeginHorizontal();
            this.otherDoguDropdown.Draw(dropdownLayoutOptions);
            this.prevOtherDoguButton.Draw(arrowLayoutOptions);
            this.nextOtherDoguButton.Draw(arrowLayoutOptions);
            this.addOtherDoguButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
        }
    }
}
