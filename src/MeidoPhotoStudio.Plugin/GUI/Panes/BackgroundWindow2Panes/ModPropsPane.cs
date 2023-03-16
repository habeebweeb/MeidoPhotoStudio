using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static MeidoPhotoStudio.Plugin.MenuFileUtility;

namespace MeidoPhotoStudio.Plugin;

public class ModPropsPane : BasePane
{
    private readonly PropManager propManager;
    private readonly Dropdown propCategoryDropdown;
    private readonly Toggle modFilterToggle;
    private readonly Toggle baseFilterToggle;
    private readonly bool isModsOnly = PropManager.ModItemsOnly;

    private Vector2 propListScrollPos;
    private List<ModItem> modPropList;
    private string currentCategory;
    private bool modItemsReady;
    private bool shouldDraw;
    private int categoryIndex;
    private bool modFilter;
    private bool baseFilter;
    private int currentListCount;

    public ModPropsPane(PropManager propManager)
    {
        this.propManager = propManager;

        modItemsReady = MenuFilesReady || PropManager.ModItemsOnly;

        var listItems = Translation.GetArray("clothing", MenuCategories);

        if (!modItemsReady)
        {
            listItems[0] = Translation.Get("systemMessage", "initializing");

            MenuFilesReadyChange += (_, _) =>
            {
                modItemsReady = true;
                propCategoryDropdown.SetDropdownItems(Translation.GetArray("clothing", MenuCategories));
            };
        }

        propCategoryDropdown = new(listItems);
        propCategoryDropdown.SelectionChange += (_, _) =>
        {
            if (!modItemsReady)
                return;

            ChangePropCategory();
        };

        if (isModsOnly)
            return;

        modFilterToggle = new(Translation.Get("background2Window", "modsToggle"));
        modFilterToggle.ControlEvent += (_, _) =>
            ChangeFilter(FilterType.Mod);

        baseFilterToggle = new(Translation.Get("background2Window", "baseToggle"));
        baseFilterToggle.ControlEvent += (_, _) =>
            ChangeFilter(FilterType.Base);
    }

    private enum FilterType
    {
        None,
        Mod,
        Base,
    }

    private string SelectedCategory =>
        MenuCategories[propCategoryDropdown.SelectedItemIndex];

    public override void Draw()
    {
        const float dropdownButtonHeight = 30f;

        var dropdownButtonWidth = isModsOnly ? 120f : 90f;

        var dropdownLayoutOptions = new[]
        {
            GUILayout.Height(dropdownButtonHeight),
            GUILayout.Width(dropdownButtonWidth),
        };

        GUILayout.BeginHorizontal();

        if (isModsOnly)
        {
            GUILayout.FlexibleSpace();
            propCategoryDropdown.Draw(dropdownLayoutOptions);
            GUILayout.FlexibleSpace();
        }
        else
        {
            GUI.enabled = modItemsReady;
            propCategoryDropdown.Draw(dropdownLayoutOptions);

            GUI.enabled = shouldDraw;
            modFilterToggle.Draw();
            baseFilterToggle.Draw();
            GUI.enabled = true;
        }

        GUILayout.EndHorizontal();

        if (shouldDraw)
        {
            var windowRect = parent.WindowRect;
            var windowHeight = windowRect.height;
            var windowWidth = windowRect.width;

            const float offsetTop = 80f;
            const int columns = 4;

            var buttonSize = windowWidth / columns - 10f;
            var positionRect = new Rect(5f, offsetTop + dropdownButtonHeight, windowWidth - 10f, windowHeight - 145f);

            var viewRect = new Rect(
                0f, 0f, buttonSize * columns, buttonSize * Mathf.Ceil(currentListCount / (float)columns) + 5);

            propListScrollPos = GUI.BeginScrollView(positionRect, propListScrollPos, viewRect);

            var modIndex = 0;

            foreach (var modItem in modPropList)
            {
                if (modFilter && !modItem.IsMod || baseFilter && modItem.IsMod)
                    continue;

                var x = modIndex % columns * buttonSize;
                var y = modIndex / columns * buttonSize;
                var iconRect = new Rect(x, y, buttonSize, buttonSize);

                if (GUI.Button(iconRect, string.Empty))
                    propManager.AddModProp(modItem);

                if (modItem.Icon == null)
                    GUI.Label(iconRect, modItem.Name);
                else
                    GUI.DrawTexture(iconRect, modItem.Icon);

                modIndex++;
            }

            GUI.EndScrollView();
        }
    }

    protected override void ReloadTranslation()
    {
        var listItems = Translation.GetArray("clothing", MenuCategories);

        if (!modItemsReady)
            listItems[0] = Translation.Get("systemMessage", "initializing");

        propCategoryDropdown.SetDropdownItems(listItems);

        if (isModsOnly)
            return;

        modFilterToggle.Label = Translation.Get("background2Window", "modsToggle");
        baseFilterToggle.Label = Translation.Get("background2Window", "baseToggle");
    }

    private void ChangeFilter(FilterType filterType)
    {
        if (updating)
            return;

        if (modFilterToggle.Value && baseFilterToggle.Value)
        {
            updating = true;
            modFilterToggle.Value = filterType is FilterType.Mod;
            baseFilterToggle.Value = filterType is FilterType.Base;
            updating = false;
        }

        modFilter = modFilterToggle.Value;
        baseFilter = baseFilterToggle.Value;

        SetListCount();
    }

    private void ChangePropCategory()
    {
        var category = SelectedCategory;

        if (currentCategory == category)
            return;

        currentCategory = category;

        categoryIndex = propCategoryDropdown.SelectedItemIndex;

        shouldDraw = categoryIndex > 0;

        if (!shouldDraw)
            return;

        propListScrollPos = Vector2.zero;

        modPropList = Constants.GetModPropList(category);

        SetListCount();
    }

    private void SetListCount() =>
        currentListCount = modFilter
            ? modPropList.Count(mod => mod.IsMod)
            : baseFilter
                ? modPropList.Count(mod => !mod.IsMod)
                : modPropList.Count;
}
