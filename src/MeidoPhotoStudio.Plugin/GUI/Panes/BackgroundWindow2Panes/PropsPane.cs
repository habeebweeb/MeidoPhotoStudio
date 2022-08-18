using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class PropsPane : BasePane
{
    private static bool handItemsReady;

    private readonly PropManager propManager;
    private readonly Dropdown doguCategoryDropdown;
    private readonly Dropdown doguDropdown;
    private readonly Button addDoguButton;
    private readonly Button nextDoguButton;
    private readonly Button prevDoguButton;
    private readonly Button nextDoguCategoryButton;
    private readonly Button prevDoguCategoryButton;

    private string currentCategory;
    private bool itemSelectorEnabled = true;

    public PropsPane(PropManager propManager)
    {
        this.propManager = propManager;

        handItemsReady = Constants.HandItemsInitialized;

        if (!handItemsReady)
            Constants.MenuFilesChange += InitializeHandItems;

        doguCategoryDropdown = new(Translation.GetArray("doguCategories", Constants.DoguCategories));
        doguCategoryDropdown.SelectionChange += (_, _) =>
            ChangeDoguCategory(SelectedCategory);

        doguDropdown = new(new[] { string.Empty });

        addDoguButton = new("+");
        addDoguButton.ControlEvent += (_, _) =>
            SpawnObject();

        nextDoguButton = new(">");
        nextDoguButton.ControlEvent += (_, _) =>
            doguDropdown.Step(1);

        prevDoguButton = new("<");
        prevDoguButton.ControlEvent += (_, _) =>
            doguDropdown.Step(-1);

        nextDoguCategoryButton = new(">");
        nextDoguCategoryButton.ControlEvent += (_, _) =>
            doguCategoryDropdown.Step(1);

        prevDoguCategoryButton = new("<");
        prevDoguCategoryButton.ControlEvent += (_, _) =>
            doguCategoryDropdown.Step(-1);

        ChangeDoguCategory(SelectedCategory);
    }

    private string SelectedCategory =>
        Constants.DoguCategories[doguCategoryDropdown.SelectedItemIndex];

    public override void Draw()
    {
        const float buttonHeight = 30;

        var arrowLayoutOptions = new[]
        {
            GUILayout.Width(buttonHeight),
            GUILayout.Height(buttonHeight),
        };

        const float dropdownButtonWidth = 120f;

        var dropdownLayoutOptions = new[]
        {
            GUILayout.Height(buttonHeight),
            GUILayout.Width(dropdownButtonWidth),
        };

        GUILayout.BeginHorizontal();
        prevDoguCategoryButton.Draw(arrowLayoutOptions);
        doguCategoryDropdown.Draw(dropdownLayoutOptions);
        nextDoguCategoryButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();

        GUI.enabled = itemSelectorEnabled;
        GUILayout.BeginHorizontal();
        doguDropdown.Draw(dropdownLayoutOptions);
        prevDoguButton.Draw(arrowLayoutOptions);
        nextDoguButton.Draw(arrowLayoutOptions);
        addDoguButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        doguCategoryDropdown.SetDropdownItems(Translation.GetArray("doguCategories", Constants.DoguCategories));

        var category = SelectedCategory;

        var translationArray =
            category == Constants.CustomDoguCategories[Constants.DoguCategory.HandItem] && !handItemsReady
                ? new[] { Translation.Get("systemMessage", "initializing") }
                : GetTranslations(category);

        doguDropdown.SetDropdownItems(translationArray);
    }

    private void InitializeHandItems(object sender, MenuFilesEventArgs args)
    {
        if (args.Type is not MenuFilesEventArgs.EventType.HandItems)
            return;

        handItemsReady = true;

        var selectedCategory = SelectedCategory;

        if (selectedCategory == Constants.CustomDoguCategories[Constants.DoguCategory.HandItem])
            ChangeDoguCategory(selectedCategory, true);
    }

    private void ChangeDoguCategory(string category, bool force = false)
    {
        if (category == currentCategory && !force)
            return;

        currentCategory = category;

        string[] translationArray;

        if (category == Constants.CustomDoguCategories[Constants.DoguCategory.HandItem] && !handItemsReady)
        {
            translationArray = new[] { Translation.Get("systemMessage", "initializing") };
            itemSelectorEnabled = false;
        }
        else
        {
            translationArray = GetTranslations(category);
            itemSelectorEnabled = true;
        }

        doguDropdown.SetDropdownItems(translationArray, 0);
    }

    private string[] GetTranslations(string category)
    {
        IEnumerable<string> itemList = Constants.DoguDict[category];

        if (category == Constants.CustomDoguCategories[Constants.DoguCategory.HandItem])
        {
            // TODO: itemList should not be reused
            itemList = itemList.Select(item =>
            {
                var handItemAsOdogu = Utility.HandItemToOdogu(item);

                return Translation.Has("propNames", handItemAsOdogu) ? handItemAsOdogu : item;
            });
        }

        var translationCategory = category == Constants.CustomDoguCategories[Constants.DoguCategory.BGSmall]
            ? "bgNames"
            : "propNames";

        return Translation.GetArray(translationCategory, itemList);
    }

    private void SpawnObject()
    {
        var assetName = Constants.DoguDict[SelectedCategory][doguDropdown.SelectedItemIndex];

        if (SelectedCategory == Constants.CustomDoguCategories[Constants.DoguCategory.BGSmall])
            propManager.AddBgProp(assetName);
        else
            propManager.AddGameProp(assetName);
    }
}
