using UnityEngine;
using System.Collections.Generic;

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

            List<string> bgList = new List<string>(Translation.GetList("bgDropdown", Constants.BGList));
            if (Constants.MyRoomCustomBGIndex >= 0)
            {
                foreach (KeyValuePair<string, string> kvp in Constants.MyRoomCustomBGList)
                {
                    bgList.Add(kvp.Value);
                }
            }

            this.bgDropdown = new Dropdown(bgList.ToArray(), theaterIndex);
            this.bgDropdown.SelectionChange += (s, a) =>
            {
                int selectedIndex = this.bgDropdown.SelectedItemIndex;
                bool isCreative = this.bgDropdown.SelectedItemIndex >= Constants.MyRoomCustomBGIndex;
                string bg = isCreative
                    ? Constants.MyRoomCustomBGList[selectedIndex - Constants.MyRoomCustomBGIndex].Key
                    : Constants.BGList[selectedIndex];

                environmentManager.ChangeBackground(bg, isCreative);
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
