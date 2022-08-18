using System.Collections.Generic;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MyRoomPropsPane : BasePane
{
    private readonly PropManager propManager;
    private readonly Dropdown propCategoryDropdown;

    private Vector2 propListScrollPos;
    private List<MyRoomItem> myRoomPropList;
    private string currentCategory;

    public MyRoomPropsPane(PropManager propManager)
    {
        this.propManager = propManager;

        propCategoryDropdown = new(Translation.GetArray("doguCategories", Constants.MyRoomPropCategories));
        propCategoryDropdown.SelectionChange += (_, _) =>
            ChangePropCategory(SelectedCategory);

        ChangePropCategory(SelectedCategory);
    }

    private string SelectedCategory =>
        Constants.MyRoomPropCategories[propCategoryDropdown.SelectedItemIndex];

    public override void Draw()
    {
        const float dropdownButtonHeight = 30f;
        const float dropdownButtonWidth = 120f;

        var dropdownLayoutOptions = new[]
        {
            GUILayout.Height(dropdownButtonHeight),
            GUILayout.Width(dropdownButtonWidth),
        };

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        propCategoryDropdown.Draw(dropdownLayoutOptions);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        var windowRect = parent.WindowRect;
        var windowHeight = windowRect.height;
        var windowWidth = windowRect.width;

        const float offsetTop = 80f;
        const int columns = 3;

        var buttonSize = windowWidth / columns - 10f;
        var listCount = myRoomPropList.Count;
        var positionRect = new Rect(5f, offsetTop + dropdownButtonHeight, windowWidth - 10f, windowHeight - 145f);
        var viewRect = new Rect(0f, 0f, buttonSize * columns, buttonSize * Mathf.Ceil(listCount / (float)columns) + 5f);

        propListScrollPos = GUI.BeginScrollView(positionRect, propListScrollPos, viewRect);

        for (var i = 0; i < listCount; i++)
        {
            var x = i % columns * buttonSize;
            var y = i / columns * buttonSize;
            var myRoomItem = myRoomPropList[i];
            var iconRect = new Rect(x, y, buttonSize, buttonSize);

            if (GUI.Button(iconRect, string.Empty))
                propManager.AddMyRoomProp(myRoomItem);

            GUI.DrawTexture(iconRect, myRoomItem.Icon);
        }

        GUI.EndScrollView();
    }

    protected override void ReloadTranslation() =>
        propCategoryDropdown.SetDropdownItems(Translation.GetArray("doguCategories", Constants.MyRoomPropCategories));

    private void ChangePropCategory(string category)
    {
        if (currentCategory == category)
            return;

        currentCategory = category;
        propListScrollPos = Vector2.zero;
        myRoomPropList = Constants.MyRoomPropDict[category];

        if (!myRoomPropList[0].Icon)
            foreach (var item in myRoomPropList)
                item.Icon = (Texture2D)MyRoomCustom.PlacementData.GetData(item.ID).GetThumbnail();
    }
}
