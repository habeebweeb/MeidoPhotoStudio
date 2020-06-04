using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    class BackgroundSelectorPane : BasePane
    {
        private EnvironmentManager environmentManager;
        private Dropdown bgDropdown;
        private Button prevBGButton;
        private Button nextBGButton;

        public BackgroundSelectorPane(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;

            int theaterIndex = Constants.BGList.FindIndex(bg => bg == "Theater");

            this.bgDropdown = new Dropdown(Translation.GetList("bgDropdown", Constants.BGList), theaterIndex);
            this.bgDropdown.SelectionChange += (s, a) =>
            {
                string bg = Constants.BGList[this.bgDropdown.SelectedItemIndex];
                environmentManager.ChangeBackground(bg);
            };

            this.prevBGButton = new Button("<");
            this.prevBGButton.ControlEvent += (s, a) => this.bgDropdown.Step(-1);

            this.nextBGButton = new Button(">");
            this.nextBGButton.ControlEvent += (s, a) => this.bgDropdown.Step(1);
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

            GUILayout.BeginHorizontal();
            this.prevBGButton.Draw(arrowLayoutOptions);
            this.bgDropdown.Draw(dropdownLayoutOptions);
            this.nextBGButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
        }
    }
}
