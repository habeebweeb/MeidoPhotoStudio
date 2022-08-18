using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class HandPresetPane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Dropdown presetCategoryDropdown;
    private readonly Button nextCategoryButton;
    private readonly Button previousCategoryButton;
    private readonly Dropdown presetDropdown;
    private readonly Button nextPresetButton;
    private readonly Button previousPresetButton;
    private readonly Button leftHandButton;
    private readonly Button rightHandButton;

    private string previousCategory;
    private bool presetListEnabled = true;

    public HandPresetPane(MeidoManager meidoManager)
    {
        Constants.CustomHandChange += OnPresetChange;

        this.meidoManager = meidoManager;

        presetCategoryDropdown = new(Constants.CustomHandGroupList.ToArray());
        presetCategoryDropdown.SelectionChange += (_, _) =>
            ChangePresetCategory();

        nextCategoryButton = new(">");
        nextCategoryButton.ControlEvent += (_, _) =>
            presetCategoryDropdown.Step(1);

        previousCategoryButton = new("<");
        previousCategoryButton.ControlEvent += (_, _) =>
            presetCategoryDropdown.Step(-1);

        presetDropdown = new(UIPresetList());

        nextPresetButton = new(">");
        nextPresetButton.ControlEvent += (_, _) =>
            presetDropdown.Step(1);

        previousPresetButton = new("<");
        previousPresetButton.ControlEvent += (_, _) =>
            presetDropdown.Step(-1);

        leftHandButton = new(Translation.Get("handPane", "leftHand"));
        leftHandButton.ControlEvent += (_, _) =>
            SetHandPreset(right: false);

        rightHandButton = new(Translation.Get("handPane", "rightHand"));
        rightHandButton.ControlEvent += (_, _) =>
            SetHandPreset(right: true);

        previousCategory = SelectedCategory;
        presetListEnabled = CurrentPresetList.Count > 0;
    }

    private string SelectedCategory =>
        Constants.CustomHandGroupList[presetCategoryDropdown.SelectedItemIndex];

    private List<string> CurrentPresetList =>
        Constants.CustomHandDict[SelectedCategory];

    private string CurrentPreset =>
        CurrentPresetList[presetDropdown.SelectedItemIndex];

    public override void Draw()
    {
        var dropdownWidth = GUILayout.Width(156f);
        var noExpandWidth = GUILayout.ExpandWidth(false);

        GUI.enabled = meidoManager.HasActiveMeido;

        GUILayout.BeginHorizontal();
        presetCategoryDropdown.Draw(dropdownWidth);
        previousCategoryButton.Draw(noExpandWidth);
        nextCategoryButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUI.enabled = GUI.enabled && presetListEnabled;

        GUILayout.BeginHorizontal();
        presetDropdown.Draw(dropdownWidth);
        previousPresetButton.Draw(noExpandWidth);
        nextPresetButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        rightHandButton.Draw();
        leftHandButton.Draw();
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        leftHandButton.Label = Translation.Get("handPane", "leftHand");
        rightHandButton.Label = Translation.Get("handPane", "rightHand");

        if (CurrentPresetList.Count is 0)
            presetDropdown.SetDropdownItems(UIPresetList());
    }

    private void ChangePresetCategory()
    {
        presetListEnabled = CurrentPresetList.Count > 0;

        if (previousCategory == SelectedCategory)
        {
            presetDropdown.SelectedItemIndex = 0;
        }
        else
        {
            previousCategory = SelectedCategory;
            presetDropdown.SetDropdownItems(UIPresetList(), 0);
        }
    }

    private void SetHandPreset(bool right = false)
    {
        if (!meidoManager.HasActiveMeido)
            return;

        meidoManager.ActiveMeido.SetHandPreset(CurrentPreset, right);
    }

    private void OnPresetChange(object sender, PresetChangeEventArgs args)
    {
        if (args == PresetChangeEventArgs.Empty)
        {
            presetCategoryDropdown.SetDropdownItems(Constants.CustomHandGroupList.ToArray(), 0);
            presetDropdown.SetDropdownItems(UIPresetList(), 0);
        }
        else
        {
            presetCategoryDropdown.SetDropdownItems(
                Constants.CustomHandGroupList.ToArray(), Constants.CustomHandGroupList.IndexOf(args.Category));

            presetDropdown.SetDropdownItems(UIPresetList(), CurrentPresetList.IndexOf(args.Path));
        }
    }

    private string[] UIPresetList() =>
        CurrentPresetList.Count is 0
            ? new[] { Translation.Get("handPane", "noPresetsMessage") }
            : CurrentPresetList.Select(Path.GetFileNameWithoutExtension).ToArray();
}
