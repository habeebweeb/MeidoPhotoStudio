using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;
    internal class MyRoomPropsPane : BasePane
    {
        private PropManager propManager;
        private Dropdown propCategoryDropdown;
        private Vector2 propListScrollPos;
        private string SelectedCategory
        {
            get => Constants.MyRoomPropCategories[this.propCategoryDropdown.SelectedItemIndex];
        }
        private List<MyRoomItem> myRoomPropList;
        private string currentCategory;

        public MyRoomPropsPane(PropManager propManager)
        {
            this.propManager = propManager;

            this.propCategoryDropdown = new Dropdown(
                Translation.GetArray("doguCategories", Constants.MyRoomPropCategories)
            );
            this.propCategoryDropdown.SelectionChange += (s, a) => ChangePropCategory(SelectedCategory);
            ChangePropCategory(SelectedCategory);
        }

        protected override void ReloadTranslation()
        {
            this.propCategoryDropdown.SetDropdownItems(
                Translation.GetArray("doguCategories", Constants.MyRoomPropCategories)
            );
        }

        public override void Draw()
        {
            float dropdownButtonHeight = 30f;
            float dropdownButtonWidth = 120f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            this.propCategoryDropdown.Draw(dropdownLayoutOptions);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            float windowHeight = Screen.height * 0.6f;

            int buttonSize = 64;
            int listCount = myRoomPropList.Count;
            int offsetLeft = 15;
            int offsetTop = 85;

            int columns = 3;

            Rect positionRect = new Rect(offsetLeft, offsetTop + dropdownButtonHeight, 220, windowHeight);
            Rect viewRect = new Rect(
                0, 0, buttonSize * columns, buttonSize * Mathf.Ceil(listCount / (float)columns) + 5
            );
            propListScrollPos = GUI.BeginScrollView(positionRect, propListScrollPos, viewRect);

            for (int i = 0; i < listCount; i++)
            {
                float x = i % columns * buttonSize;
                float y = i / columns * buttonSize;
                MenuFileUtility.MyRoomItem myRoomItem = myRoomPropList[i];
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
            this.myRoomPropList = Constants.MyRoomPropDict[category];
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
