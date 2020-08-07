using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BackgroundSelectorPane : BasePane
    {
        private EnvironmentManager environmentManager;
        private Dropdown bgDropdown;
        private Button prevBGButton;
        private Button nextBGButton;

        public BackgroundSelectorPane(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;

            int theaterIndex = Constants.BGList.FindIndex(bg => bg == "Theater");

            List<string> bgList = new List<string>(Translation.GetList("bgNames", Constants.BGList));
            if (Constants.MyRoomCustomBGIndex >= 0)
            {
                bgList.AddRange(Constants.MyRoomCustomBGList.Select(kvp => kvp.Value));
            }

            this.bgDropdown = new Dropdown(bgList.ToArray(), theaterIndex);
            this.bgDropdown.SelectionChange += (s, a) =>
            {
                if (updating) return;
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

        protected override void ReloadTranslation()
        {
            List<string> bgList = new List<string>(Translation.GetList("bgNames", Constants.BGList));
            if (Constants.MyRoomCustomBGIndex >= 0)
            {
                bgList.AddRange(Constants.MyRoomCustomBGList.Select(kvp => kvp.Value));
            }

            updating = true;
            this.bgDropdown.SetDropdownItems(bgList.ToArray());
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

            GUILayout.BeginHorizontal();
            this.prevBGButton.Draw(arrowLayoutOptions);
            this.bgDropdown.Draw(dropdownLayoutOptions);
            this.nextBGButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
        }
    }
}
