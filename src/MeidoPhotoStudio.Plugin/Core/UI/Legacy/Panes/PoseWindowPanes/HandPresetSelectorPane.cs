using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class HandPresetSelectorPane : BasePane
{
    private readonly HandPresetRepository handPresetRepository;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Dropdown<string> presetCategoryDropdown;
    private readonly Dropdown<HandPresetModel> presetDropdown;
    private readonly Button applyLeftHandButton;
    private readonly Button applyRightHandButton;
    private readonly Button swapHandsButton;
    private readonly SubPaneHeader savePresetToggle;
    private readonly Framework.UI.Legacy.ComboBox handPresetCategoryComboBox;
    private readonly TextField handPresetNameTextField;
    private readonly Button saveLeftPresetButton;
    private readonly Button saveRightPresetButton;
    private readonly Header handPresetDirectoryHeader;
    private readonly Header handPresetFilenameHeader;
    private readonly Label noPresetsLabel;
    private readonly Button refreshButton;
    private readonly Label savedHandPresetLabel;
    private readonly SearchBar<HandPresetModel> searchBar;
    private readonly Toggle autoApplyLeftToggle;
    private readonly Toggle autoApplyRightToggle;
    private readonly LazyStyle noPresetsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private bool showSaveHandPresetLabel;
    private float saveTime;

    public HandPresetSelectorPane(
        Translation translation,
        HandPresetRepository handPresetRepository,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.handPresetRepository = handPresetRepository ?? throw new ArgumentNullException(nameof(handPresetRepository));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.handPresetRepository.AddedHandPreset += OnHandPresetAdded;
        this.handPresetRepository.Refreshed += OnHandPresetRepositoryRefreshed;

        translation.Initialized += OnTranslationInitialized;

        searchBar = new(SearchSelector, Formatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "handPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        presetCategoryDropdown = new(PresetCategoryList());
        presetCategoryDropdown.SelectionChanged += OnPresetCategoryChanged;

        presetDropdown = new(PresetList(), formatter: Formatter);
        presetDropdown.SelectionChanged += OnPresetSelectionChanged;

        applyLeftHandButton = new(new LocalizableGUIContent(translation, "handPane", "leftHand"));
        applyLeftHandButton.ControlEvent += OnApplyLeftButtonPushed;

        applyRightHandButton = new(new LocalizableGUIContent(translation, "handPane", "rightHand"));
        applyRightHandButton.ControlEvent += OnApplyRightButtonPushed;

        swapHandsButton = new(new LocalizableGUIContent(translation, "handPane", "swapHands"));
        swapHandsButton.ControlEvent += OnSwapButtonPushed;

        savePresetToggle = new(new LocalizableGUIContent(translation, "handPane", "saveToggle"), false);
        handPresetCategoryComboBox = new(this.handPresetRepository.Categories)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "handPane", "categorySearchBarPlaceholder"),
        };

        handPresetNameTextField = new()
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "handPane", "nameTextFieldPlaceholder"),
        };

        saveLeftPresetButton = new(new LocalizableGUIContent(translation, "handPane", "saveLeftButton"));
        saveLeftPresetButton.ControlEvent += OnSaveLeftPresetButtonPushed;

        saveRightPresetButton = new(new LocalizableGUIContent(translation, "handPane", "saveRightButton"));
        saveRightPresetButton.ControlEvent += OnSaveRightPresetButtonPushed;

        refreshButton = new(new LocalizableGUIContent(translation, "handPane", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        handPresetDirectoryHeader = new(new LocalizableGUIContent(translation, "handPane", "categoryHeader"));
        handPresetFilenameHeader = new(new LocalizableGUIContent(translation, "handPane", "nameHeader"));

        noPresetsLabel = new(new LocalizableGUIContent(translation, "handPane", "noPresetsMessage"));
        savedHandPresetLabel = new(new LocalizableGUIContent(translation, "handPane", "savedHandPresetLabel"));

        autoApplyLeftToggle = new(new LocalizableGUIContent(translation, "handPane", "autoApplyLeft"));
        autoApplyRightToggle = new(new LocalizableGUIContent(translation, "handPane", "autoApplyRight"));

        IDropdownItem Formatter(HandPresetModel preset, int index) =>
            new LabelledDropdownItem($"{index + 1}: {preset.Name}");

        IEnumerable<HandPresetModel> SearchSelector(string query) =>
            handPresetRepository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        void OnTranslationInitialized(object sender, EventArgs e) =>
            searchBar.Reformat();
    }

    private CharacterController Character =>
        characterSelectionController.Current;

    private IKController IKController =>
        characterSelectionController.Current?.IK;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        if (!presetCategoryDropdown.Any())
        {
            noPresetsLabel.Draw(noPresetsLabelStyle);
        }
        else if (!presetDropdown.Any())
        {
            DrawTextFieldWithScrollBarOffset(searchBar);

            DrawDropdown(presetCategoryDropdown);

            noPresetsLabel.Draw(noPresetsLabelStyle);
        }
        else
        {
            DrawTextFieldWithScrollBarOffset(searchBar);

            DrawDropdown(presetCategoryDropdown);
            DrawDropdown(presetDropdown);
        }

        UIUtility.DrawBlackLine();

        GUI.enabled = enabled && presetDropdown.Any();

        GUILayout.BeginHorizontal();

        autoApplyRightToggle.Draw();
        autoApplyLeftToggle.Draw();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        applyRightHandButton.Draw();
        applyLeftHandButton.Draw();

        GUILayout.EndHorizontal();

        GUI.enabled = enabled;

        swapHandsButton.Draw();

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        savePresetToggle.Draw();

        refreshButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        if (savePresetToggle.Enabled)
            DrawAddHandPreset();

        void DrawAddHandPreset()
        {
            handPresetDirectoryHeader.Draw();
            DrawComboBox(handPresetCategoryComboBox);

            handPresetFilenameHeader.Draw();
            DrawTextFieldWithScrollBarOffset(handPresetNameTextField);

            UIUtility.DrawBlackLine();

            GUILayout.BeginHorizontal();

            saveRightPresetButton.Draw();
            saveLeftPresetButton.Draw();

            GUILayout.EndHorizontal();

            if (!showSaveHandPresetLabel)
                return;

            if (Time.time - saveTime >= 2.5f)
            {
                showSaveHandPresetLabel = false;

                return;
            }

            savedHandPresetLabel.Draw();
        }
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<HandPresetModel> e)
    {
        var preset = e.Item;

        if (!string.Equals(preset.Category, presetCategoryDropdown.SelectedItem))
        {
            var categoryIndex = presetCategoryDropdown.IndexOf(preset.Category, StringComparer.Ordinal);

            if (presetCategoryDropdown.SelectedItemIndex != categoryIndex)
                presetCategoryDropdown.SelectedItemIndex = categoryIndex;
        }

        var presetIndex = presetDropdown.IndexOf(preset);

        if (presetIndex < 0)
            return;

        presetDropdown.SelectedItemIndex = presetIndex;
    }

    private void OnHandPresetAdded(object sender, AddedHandPresetEventArgs e)
    {
        var currentCategory = presetCategoryDropdown.SelectedItem;

        if (!presetCategoryDropdown.Contains(e.HandPreset.Category))
        {
            presetCategoryDropdown.SetItemsWithoutNotify(PresetCategoryList());
            handPresetCategoryComboBox.SetItems(handPresetRepository.Categories);
        }

        var currentCategoryIndex = presetCategoryDropdown.IndexOf(currentCategory, StringComparer.Ordinal);

        presetCategoryDropdown.SetSelectedIndexWithoutNotify(currentCategoryIndex);

        if (!string.Equals(currentCategory, e.HandPreset.Category, StringComparison.Ordinal))
            return;

        var currentPreset = presetDropdown.SelectedItem;

        presetDropdown.SetItemsWithoutNotify(PresetList());

        var currentpresetIndex = presetDropdown.IndexOf(currentPreset);

        presetDropdown.SetSelectedIndexWithoutNotify(currentpresetIndex);
    }

    private void OnHandPresetRepositoryRefreshed(object sender, EventArgs e)
    {
        var newCategories = PresetCategoryList().ToArray();

        handPresetCategoryComboBox.SetItems(newCategories);

        if (handPresetRepository.ContainsCategory(presetCategoryDropdown.SelectedItem))
        {
            var currentCategory = presetCategoryDropdown.SelectedItem;

            var categoryIndex = newCategories.IndexOf(currentCategory, StringComparer.Ordinal);

            if (categoryIndex < 0)
            {
                presetCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
                presetDropdown.SetItemsWithoutNotify(PresetList(), 0);

                return;
            }

            presetCategoryDropdown.SetItemsWithoutNotify(newCategories, categoryIndex);

            var currentPresetModel = presetDropdown.SelectedItem;
            var newPresets = PresetList().ToArray();
            var presetIndex = newPresets.FindIndex(newPreset => currentPresetModel.ID == newPreset.ID);

            if (presetIndex < 0)
                presetIndex = 0;

            presetDropdown.SetItemsWithoutNotify(newPresets, presetIndex);
        }
        else
        {
            presetCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
        }
    }

    private void OnPresetSelectionChanged(object sender, DropdownEventArgs<HandPresetModel> e)
    {
        if (autoApplyLeftToggle.Value)
            ApplyPreset(HandOrFootType.HandLeft);

        if (autoApplyRightToggle.Value)
            ApplyPreset(HandOrFootType.HandRight);
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

        characterUndoRedoService[Character].StartPoseChange();
        IKController.SwapHands();
        characterUndoRedoService[Character].EndPoseChange();
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

        showSaveHandPresetLabel = true;
        saveTime = Time.time;
    }

    private void ApplyPreset(HandOrFootType type)
    {
        if (IKController is null)
            return;

        if (presetDropdown.SelectedItem is null)
            return;

        characterUndoRedoService[Character].StartPoseChange();
        IKController.ApplyHandOrFootPreset(presetDropdown.SelectedItem, type);
        characterUndoRedoService[Character].EndPoseChange();
    }

    private IEnumerable<string> PresetCategoryList() =>
        handPresetRepository.Categories
            .OrderBy(category => !string.Equals(category, handPresetRepository.RootCategoryName, StringComparison.Ordinal))
            .ThenBy(category => category, new WindowsLogicalStringComparer());

    private IEnumerable<HandPresetModel> PresetList() =>
        presetCategoryDropdown.SelectedItem is null
            ? []
            : handPresetRepository[presetCategoryDropdown.SelectedItem]
                .OrderBy(static preset => preset.Name, new WindowsLogicalStringComparer());
}
