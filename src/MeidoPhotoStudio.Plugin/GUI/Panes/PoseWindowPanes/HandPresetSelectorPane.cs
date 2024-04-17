using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class HandPresetSelectorPane : BasePane
{
    private readonly HandPresetRepository handPresetRepository;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle paneHeader;
    private readonly Dropdown2<string> presetCategoryDropdown;
    private readonly Dropdown2<HandPresetModel> presetDropdown;
    private readonly Button applyLeftHandButton;
    private readonly Button applyRightHandButton;
    private readonly Button swapHandsButton;
    private readonly Toggle savePresetToggle;
    private readonly ComboBox handPresetCategoryComboBox;
    private readonly TextField handPresetNameTextField;
    private readonly Button saveLeftPresetButton;
    private readonly Button saveRightPresetButton;
    private readonly Header handPresetDirectoryHeader;
    private readonly Header handPresetFilenameHeader;
    private readonly Label noPresetsLabel;
    private readonly Button refreshButton;

    public HandPresetSelectorPane(
        HandPresetRepository handPresetRepository,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.handPresetRepository = handPresetRepository ?? throw new ArgumentNullException(nameof(handPresetRepository));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.handPresetRepository.AddedHandPreset += OnHandPresetAdded;
        this.handPresetRepository.Refreshed += OnHandPresetRepositoryRefreshed;

        paneHeader = new(Translation.Get("handPane", "header"), true);

        presetCategoryDropdown = new(PresetCategoryList());
        presetCategoryDropdown.SelectionChanged += OnPresetCategoryChanged;

        presetDropdown = new(PresetList(), formatter: (preset, index) => $"{index + 1}: {preset.Name}");

        applyLeftHandButton = new(Translation.Get("handPane", "leftHand"));
        applyLeftHandButton.ControlEvent += OnApplyLeftButtonPushed;

        applyRightHandButton = new(Translation.Get("handPane", "rightHand"));
        applyRightHandButton.ControlEvent += OnApplyRightButtonPushed;

        swapHandsButton = new(Translation.Get("handPane", "swapHands"));
        swapHandsButton.ControlEvent += OnSwapButtonPushed;

        savePresetToggle = new(Translation.Get("handPane", "saveToggle"));
        handPresetCategoryComboBox = new(this.handPresetRepository.Categories.ToArray());
        handPresetNameTextField = new();

        saveLeftPresetButton = new(Translation.Get("handPane", "saveLeftButton"));
        saveLeftPresetButton.ControlEvent += OnSaveLeftPresetButtonPushed;

        saveRightPresetButton = new(Translation.Get("handPane", "saveRightButton"));
        saveRightPresetButton.ControlEvent += OnSaveRightPresetButtonPushed;

        refreshButton = new(Translation.Get("handPane", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        handPresetDirectoryHeader = new(Translation.Get("handPane", "categoryHeader"));
        handPresetFilenameHeader = new(Translation.Get("handPane", "nameHeader"));

        noPresetsLabel = new(Translation.Get("handPane", "noPresetsMessage"));
    }

    private IKController IKController =>
        characterSelectionController.Current?.IK;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        GUI.enabled = enabled;

        if (!presetCategoryDropdown.Any())
        {
            noPresetsLabel.Draw();
        }
        else if (!presetDropdown.Any())
        {
            DrawDropdown(presetCategoryDropdown);

            noPresetsLabel.Draw();
        }
        else
        {
            DrawDropdown(presetCategoryDropdown);
            DrawDropdown(presetDropdown);
        }

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        savePresetToggle.Draw();

        GUILayout.FlexibleSpace();

        refreshButton.Draw();

        GUILayout.EndHorizontal();

        if (savePresetToggle.Value)
            DrawAddHandPreset();

        MpsGui.BlackLine();

        GUI.enabled = enabled && presetDropdown.Any();
        GUILayout.BeginHorizontal();

        applyRightHandButton.Draw();
        applyLeftHandButton.Draw();

        GUILayout.EndHorizontal();

        GUI.enabled = enabled;

        swapHandsButton.Draw();

        void DrawAddHandPreset()
        {
            var parentWidth = parent.WindowRect.width;
            var width = GUILayout.Width(parentWidth - 75f);

            handPresetDirectoryHeader.Draw();
            handPresetCategoryComboBox.Draw(width);

            handPresetFilenameHeader.Draw();
            handPresetNameTextField.Draw(width);

            GUILayout.BeginHorizontal();

            saveRightPresetButton.Draw();
            saveLeftPresetButton.Draw();

            GUILayout.EndHorizontal();
        }

        static void DrawDropdown<T>(Dropdown2<T> dropdown)
        {
            GUILayout.BeginHorizontal();

            const float dropdownButtonWidth = 175f;

            dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.CyclePrevious();

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.CycleNext();

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("handPane", "header");
        applyLeftHandButton.Label = Translation.Get("handPane", "leftHand");
        applyRightHandButton.Label = Translation.Get("handPane", "rightHand");
        swapHandsButton.Label = Translation.Get("handPane", "swapHands");
        savePresetToggle.Label = Translation.Get("handPane", "saveToggle");
        saveLeftPresetButton.Label = Translation.Get("handPane", "saveLeftButton");
        saveRightPresetButton.Label = Translation.Get("handPane", "saveRightButton");
        refreshButton.Label = Translation.Get("handPane", "refreshButton");
        handPresetDirectoryHeader.Text = Translation.Get("handPane", "categoryHeader");
        handPresetFilenameHeader.Text = Translation.Get("handPane", "nameHeader");
        noPresetsLabel.Text = Translation.Get("handPane", "noPresetsMessage");
    }

    private void OnHandPresetAdded(object sender, AddedHandPresetEventArgs e)
    {
        if (!presetCategoryDropdown.Contains(e.HandPreset.Category))
        {
            presetCategoryDropdown.SetItemsWithoutNotify(PresetCategoryList());
            handPresetCategoryComboBox.BaseDropDown.SetDropdownItemsWithoutNotify([.. handPresetRepository.Categories], 0);
        }

        if (!string.Equals(presetCategoryDropdown.SelectedItem, e.HandPreset.Category, StringComparison.Ordinal))
            presetCategoryDropdown.SelectedItemIndex = presetCategoryDropdown
                .IndexOf(category => string.Equals(category, e.HandPreset.Category, StringComparison.Ordinal));

        presetDropdown.SetSelectedIndexWithoutNotify(presetDropdown.IndexOf(preset => e.HandPreset.ID == preset.ID));
    }

    private void OnHandPresetRepositoryRefreshed(object sender, EventArgs e)
    {
        if (handPresetRepository.ContainsCategory(presetCategoryDropdown.SelectedItem))
        {
            var currentCategory = presetCategoryDropdown.SelectedItem;
            var newCategories = PresetCategoryList().ToArray();

            handPresetCategoryComboBox.SetDropdownItems(newCategories);

            var categoryIndex = newCategories.IndexOf(category => string.Equals(currentCategory, category, StringComparison.Ordinal));

            if (categoryIndex < 0)
            {
                presetCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
                presetDropdown.SetItemsWithoutNotify(PresetList(), 0);

                return;
            }

            presetCategoryDropdown.SetItemsWithoutNotify(newCategories, categoryIndex);

            var currentPresetModel = presetDropdown.SelectedItem;
            var newPresets = PresetList().ToArray();
            var presetIndex = newPresets.IndexOf(preset => preset == currentPresetModel);

            if (presetIndex < 0)
                presetIndex = 0;

            presetDropdown.SetItemsWithoutNotify(newPresets, presetIndex);
        }
        else
        {
            var newCategories = PresetCategoryList().ToArray();

            presetCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
            handPresetCategoryComboBox.BaseDropDown.SetDropdownItemsWithoutNotify(newCategories);
        }
    }

    private void OnRefreshButtonPushed(object sender, EventArgs e) =>
        handPresetRepository.Refresh();

    private void OnSaveLeftPresetButtonPushed(object sender, EventArgs e) =>
        SavePreset(HandOrFootType.HandLeft);

    private void OnSaveRightPresetButtonPushed(object sender, EventArgs e) =>
        SavePreset(HandOrFootType.HandRight);

    private void OnApplyLeftButtonPushed(object sender, EventArgs e)
    {
        if (IKController is null)
            return;

        ApplyPreset(HandOrFootType.HandLeft);
    }

    private void OnApplyRightButtonPushed(object sender, EventArgs e)
    {
        if (IKController is null)
            return;

        ApplyPreset(HandOrFootType.HandRight);
    }

    private void OnSwapButtonPushed(object sender, EventArgs e)
    {
        if (IKController is null)
            return;

        IKController.SwapHands();
    }

    private void OnPresetCategoryChanged(object sender, EventArgs e) =>
        presetDropdown.SetItems(PresetList(), 0);

    private void SavePreset(HandOrFootType type)
    {
        if (IKController is null)
            return;

        var presetData = IKController.GetHandOrFootPreset(type);
        var category = handPresetCategoryComboBox.Value;
        var name = handPresetNameTextField.Value;

        if (string.IsNullOrEmpty(category))
            category = handPresetRepository.RootCategoryName;

        if (string.IsNullOrEmpty(name))
            name = "hand_preset";

        handPresetRepository.Add(presetData, category, name);

        handPresetNameTextField.Value = string.Empty;
    }

    private void ApplyPreset(HandOrFootType type)
    {
        if (IKController is null)
            return;

        if (presetDropdown.SelectedItem is null)
            return;

        IKController.ApplyHandOrFootPreset(presetDropdown.SelectedItem, type);
    }

    private IEnumerable<string> PresetCategoryList() =>
        handPresetRepository.Categories
            .OrderBy(category => !string.Equals(category, handPresetRepository.RootCategoryName, StringComparison.Ordinal))
            .ThenBy(category => category, new WindowsLogicalStringComparer());

    private IEnumerable<HandPresetModel> PresetList() =>
        presetCategoryDropdown.SelectedItem is null
            ? []
            : handPresetRepository[presetCategoryDropdown.SelectedItem]
                .OrderBy(preset => preset.Name, new WindowsLogicalStringComparer());
}
