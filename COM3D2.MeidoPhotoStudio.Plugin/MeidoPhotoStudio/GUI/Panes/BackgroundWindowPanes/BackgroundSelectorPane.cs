using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BackgroundSelectorPane : BasePane
    {
        private readonly EnvironmentManager environmentManager;
        private readonly Dropdown bgDropdown;
        private readonly Button prevBGButton;
        private readonly Button nextBGButton;

        public BackgroundSelectorPane(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;

            int theaterIndex = Constants.BGList.FindIndex(bg => bg == "Theater");

            List<string> bgList = new List<string>(Translation.GetList("bgNames", Constants.BGList));
            if (Constants.MyRoomCustomBGIndex >= 0)
            {
                bgList.AddRange(Constants.MyRoomCustomBGList.Select(kvp => kvp.Value));
            }

            bgDropdown = new Dropdown(bgList.ToArray(), theaterIndex);
            bgDropdown.SelectionChange += (s, a) => ChangeBackground();

            prevBGButton = new Button("<");
            prevBGButton.ControlEvent += (s, a) => bgDropdown.Step(-1);

            nextBGButton = new Button(">");
            nextBGButton.ControlEvent += (s, a) => bgDropdown.Step(1);
        }

        protected override void ReloadTranslation()
        {
            List<string> bgList = new List<string>(Translation.GetList("bgNames", Constants.BGList));
            if (Constants.MyRoomCustomBGIndex >= 0)
            {
                bgList.AddRange(Constants.MyRoomCustomBGList.Select(kvp => kvp.Value));
            }

            updating = true;
            bgDropdown.SetDropdownItems(bgList.ToArray());
            updating = false;
        }

        public override void Draw()
        {
            const float buttonHeight = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(buttonHeight),
                GUILayout.Height(buttonHeight)
            };

            const float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(buttonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUILayout.BeginHorizontal();
            prevBGButton.Draw(arrowLayoutOptions);
            bgDropdown.Draw(dropdownLayoutOptions);
            nextBGButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
        }

        private void ChangeBackground()
        {
            if (updating) return;
            int selectedIndex = bgDropdown.SelectedItemIndex;
            bool isCreative = bgDropdown.SelectedItemIndex >= Constants.MyRoomCustomBGIndex;
            string bg = isCreative
                ? Constants.MyRoomCustomBGList[selectedIndex - Constants.MyRoomCustomBGIndex].Key
                : Constants.BGList[selectedIndex];

            environmentManager.ChangeBackground(bg, isCreative);
        }
    }
}
