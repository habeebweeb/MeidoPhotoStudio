using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MyRoomPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly MyRoomPropRepository myRoomPropRepository;
    private readonly IconCache iconCache;
    private readonly Dropdown propCategoryDropdown;
    private readonly int[] categories;

    private Vector2 scrollPosition;
    private IEnumerable<MyRoomPropModel> currentPropList;

    public MyRoomPropsPane(
        PropService propService, MyRoomPropRepository myRoomPropRepository, IconCache iconCache)
    {
        this.propService = propService ?? throw new System.ArgumentNullException(nameof(propService));
        this.myRoomPropRepository = myRoomPropRepository ?? throw new System.ArgumentNullException(nameof(myRoomPropRepository));
        this.iconCache = iconCache ?? throw new System.ArgumentNullException(nameof(iconCache));

        categories = new[] { -1 }.Concat(myRoomPropRepository.CategoryIDs.OrderBy(id => id)).ToArray();

        propCategoryDropdown = new(categories
            .Select(id => Translation.Get("myRoomPropCategories", id.ToString()))
            .ToArray());

        propCategoryDropdown.SelectionChange += OnPropCategoryDropdownChanged;

        UpdateCurrentPropList();
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);

        MpsGui.BlackLine();

        DrawPropList();

        static void DrawDropdown(Dropdown dropdown)
        {
            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            const float dropdownButtonWidth = 185f;

            var dropdownLayoutOptions = new[]
            {
                GUILayout.Width(dropdownButtonWidth),
            };

            GUILayout.BeginHorizontal();

            dropdown.Draw(dropdownLayoutOptions);

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.Step(-1);

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.Step(1);

            GUILayout.EndHorizontal();
        }

        void DrawPropList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            const float buttonSize = 70f;

            var buttonStyle = new GUIStyle(GUI.skin.button) { padding = new(0, 0, 0, 0) };
            var buttonLayoutOptions = new GUILayoutOption[]
            {
                GUILayout.Width(buttonSize), GUILayout.Height(buttonSize),
            };

            foreach (var propChunk in currentPropList.Chunk(3))
            {
                GUILayout.BeginHorizontal();

                foreach (var prop in propChunk)
                {
                    var icon = iconCache.GetMyRoomIcon(prop);
                    var clicked = GUILayout.Button(icon, buttonStyle, buttonLayoutOptions);

                    if (clicked)
                        propService.Add(prop);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        propCategoryDropdown.SetDropdownItemsWithoutNotify(
            categories.Select(id => Translation.Get("myRoomPropCategories", id.ToString())).ToArray());
    }

    private void UpdateCurrentPropList()
    {
        var currentCategory = categories[propCategoryDropdown.SelectedItemIndex];

        if (currentCategory is -1)
        {
            currentPropList = Enumerable.Empty<MyRoomPropModel>();

            return;
        }

        scrollPosition = Vector2.zero;

        currentPropList = myRoomPropRepository[currentCategory];
    }

    private void OnPropCategoryDropdownChanged(object sender, System.EventArgs e) =>
        UpdateCurrentPropList();
}
