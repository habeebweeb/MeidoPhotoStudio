using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;
    internal class MyRoomPropsPane : BasePane
    {
        private readonly PropManager propManager;
        private readonly Dropdown propCategoryDropdown;
        private Vector2 propListScrollPos;
        private string SelectedCategory => Constants.MyRoomPropCategories[propCategoryDropdown.SelectedItemIndex];
        private List<MyRoomItem> myRoomPropList;
        private string currentCategory;

        public MyRoomPropsPane(PropManager propManager)
        {
            this.propManager = propManager;

            propCategoryDropdown = new Dropdown(Translation.GetArray("doguCategories", Constants.MyRoomPropCategories));
            propCategoryDropdown.SelectionChange += (s, a) => ChangePropCategory(SelectedCategory);
            ChangePropCategory(SelectedCategory);
        }

        protected override void ReloadTranslation()
        {
            propCategoryDropdown.SetDropdownItems(
                Translation.GetArray("doguCategories", Constants.MyRoomPropCategories)
            );
        }

        public override void Draw()
        {
            const float dropdownButtonHeight = 30f;
            const float dropdownButtonWidth = 120f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            propCategoryDropdown.Draw(dropdownLayoutOptions);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            float windowHeight = Screen.height * 0.6f;

            const int buttonSize = 64;
            const int offsetLeft = 15;
            const int offsetTop = 85;
            const int columns = 3;

            int listCount = myRoomPropList.Count;

            Rect positionRect = new Rect(offsetLeft, offsetTop + dropdownButtonHeight, 220, windowHeight);
            Rect viewRect = new Rect(
                0, 0, buttonSize * columns, (buttonSize * Mathf.Ceil(listCount / (float)columns)) + 5
            );
            propListScrollPos = GUI.BeginScrollView(positionRect, propListScrollPos, viewRect);

            for (int i = 0; i < listCount; i++)
            {
                float x = i % columns * buttonSize;
                float y = i / columns * buttonSize;
                MyRoomItem myRoomItem = myRoomPropList[i];
                Rect iconRect = new Rect(x, y, buttonSize, buttonSize);
                if (GUI.Button(iconRect, "")) propManager.SpawnMyRoomProp(myRoomItem);
                GUI.DrawTexture(iconRect, myRoomItem.Icon);
            }

            GUI.EndScrollView();
            GUILayout.Space(windowHeight);
        }

        private void ChangePropCategory(string category)
        {
            if (currentCategory == category) return;
            currentCategory = category;
            propListScrollPos = Vector2.zero;
            myRoomPropList = Constants.MyRoomPropDict[category];
            if (myRoomPropList[0].Icon == null)
            {
                foreach (MyRoomItem item in myRoomPropList)
                {
                    item.Icon = (Texture2D)MyRoomCustom.PlacementData.GetData(item.ID).GetThumbnail();
                }
            }
        }
    }
}
