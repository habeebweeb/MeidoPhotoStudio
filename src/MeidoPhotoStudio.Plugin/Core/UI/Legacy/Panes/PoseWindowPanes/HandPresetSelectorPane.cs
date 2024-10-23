using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class HandPresetSelectorPane : BasePane
{
    private readonly HandPresetRepository handPresetRepository;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly PaneHeader paneHeader;
    private readonly Dropdown<string> presetCategoryDropdown;
    private readonly Dropdown<HandPresetModel> presetDropdown;
    private readonly Button applyLeftHandButton;
    private readonly Button applyRightHandButton;
    private readonly Button swapHandsButton;
    private readonly Toggle savePresetToggle;
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

    private bool showSaveHandPresetLabel;
    private float saveTime;

    public HandPresetSelectorPane(
        HandPresetRepository handPresetRepository,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.handPresetRepository = handPresetRepository ?? throw new ArgumentNullException(nameof(handPresetRepository));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.handPresetRepository.AddedHandPreset += OnHandPresetAdded;
        this.handPresetRepository.Refreshed += OnHandPresetRepositoryRefreshed;

        paneHeader = new(Translation.Get("handPane", "header"), true);

        searchBar = new(SearchSelector, Formatter)
        {
            Placeholder = Translation.Get("handPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        presetCategoryDropdown = new(PresetCategoryList());
        presetCategoryDropdown.SelectionChanged += OnPresetCategoryChanged;

        presetDropdown = new(PresetList(), formatter: Formatter);

        applyLeftHandButton = new(Translation.Get("handPane", "leftHand"));
        applyLeftHandButton.ControlEvent += OnApplyLeftButtonPushed;

        applyRightHandButton = new(Translation.Get("handPane", "rightHand"));
        applyRightHandButton.ControlEvent += OnApplyRightButtonPushed;

        swapHandsButton = new(Translation.Get("handPane", "swapHands"));
        swapHandsButton.ControlEvent += OnSwapButtonPushed;

        savePresetToggle = new(Translation.Get("handPane", "saveToggle"));
        handPresetCategoryComboBox = new(this.handPresetRepository.Categories)
        {
            Placeholder = Translation.Get("handPane", "categorySearchBarPlaceholder"),
        };

        handPresetNameTextField = new()
        {
            Placeholder = Translation.Get("handPane", "nameTextFieldPlaceholder"),
        };

        saveLeftPresetButton = new(Translation.Get("handPane", "saveLeftButton"));
        saveLeftPresetButton.ControlEvent += OnSaveLeftPresetButtonPushed;

        saveRightPresetButton = new(Translation.Get("handPane", "saveRightButton"));
        saveRightPresetButton.ControlEvent += OnSaveRightPresetButtonPushed;

        refreshButton = new(Translation.Get("handPane", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        handPresetDirectoryHeader = new(Translation.Get("handPane", "categoryHeader"));
        handPresetFilenameHeader = new(Translation.Get("handPane", "nameHeader"));

        noPresetsLabel = new(Translation.Get("handPane", "noPresetsMessage"));
        savedHandPresetLabel = new(Translation.Get("handPane", "savedHandPresetLabel"));

        IDropdownItem Formatter(HandPresetModel preset, int index) =>
            new LabelledDropdownItem($"{index + 1}: {preset.Name}");

        IEnumerable<HandPresetModel> SearchSelector(string query) =>
            handPresetRepository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    private CharacterController Character =>
        characterSelectionController.Current;

    private IKController IKController =>
        characterSelectionController.Current?.IK;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUI.enabled = enabled;

        if (!presetCategoryDropdown.Any())
        {
            noPresetsLabel.Draw();
        }
        else if (!presetDropdown.Any())
        {
            DrawTextFieldWithScrollBarOffset(searchBar);

            DrawDropdown(presetCategoryDropdown);

            noPresetsLabel.Draw();
        }
        else
        {
            DrawTextFieldWithScrollBarOffset(searchBar);

            DrawDropdown(presetCategoryDropdown);
            DrawDropdown(presetDropdown);
        }

        MpsGui.BlackLine();

        GUI.enabled = enabled && presetDropdown.Any();
        GUILayout.BeginHorizontal();

        applyRightHandButton.Draw();
        applyLeftHandButton.Draw();

        GUILayout.EndHorizontal();

        GUI.enabled = enabled;

        swapHandsButton.Draw();

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        savePresetToggle.Draw();

        GUILayout.FlexibleSpace();

        refreshButton.Draw();

        GUILayout.EndHorizontal();

        if (savePresetToggle.Value)
            DrawAddHandPreset();

        void DrawAddHandPreset()
        {
            handPresetDirectoryHeader.Draw();
            DrawComboBox(handPresetCategoryComboBox);

            handPresetFilenameHeader.Draw();
            DrawTextFieldWithScrollBarOffset(handPresetNameTextField);

            MpsGui.BlackLine();

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
        handPresetCategoryComboBox.Placeholder = Translation.Get("handPane", "categorySearchBarPlaceholder");
        handPresetNameTextField.Placeholder = Translation.Get("handPane", "nameTextFieldPlaceholder");
        noPresetsLabel.Text = Translation.Get("handPane", "noPresetsMessage");
        savedHandPresetLabel.Text = Translation.Get("handPane", "savedHandPresetLabel");
        searchBar.Placeholder = Translation.Get("handPane", "searchBarPlaceholder");
        searchBar.Reformat();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<HandPresetModel> e)
    {
        var preset = e.Item;

        if (!string.Equals(preset.Category, presetCategoryDropdown.SelectedItem))
        {
            var categoryIndex = presetCategoryDropdown
                .FindIndex(category => string.Equals(preset.Category, category, StringComparison.Ordinal));

            if (presetCategoryDropdown.SelectedItemIndex != categoryIndex)
                presetCategoryDropdown.SelectedItemIndex = categoryIndex;
        }

        var presetIndex = presetDropdown.IndexOf(preset);

        if (presetIndex < 0)
            return;

        presetDropdown.SetSelectedIndexWithoutNotify(presetIndex);
    }

    private void OnHandPresetAdded(object sender, AddedHandPresetEventArgs e)
    {
        var currentCategory = presetCategoryDropdown.SelectedItem;

        if (!presetCategoryDropdown.Contains(e.HandPreset.Category))
        {
            presetCategoryDropdown.SetItemsWithoutNotify(PresetCategoryList());
            handPresetCategoryComboBox.SetItems(handPresetRepository.Categories);
        }

        var currentCategoryIndex = presetCategoryDropdown
            .IndexOf(category => string.Equals(category, currentCategory, StringComparison.Ordinal));

        presetCategoryDropdown.SetSelectedIndexWithoutNotify(currentCategoryIndex);

        if (!string.Equals(currentCategory, e.HandPreset.Category, StringComparison.Ordinal))
            return;

        var currentPreset = presetDropdown.SelectedItem;

        presetDropdown.SetItemsWithoutNotify(PresetList());

        var currentpresetIndex = presetDropdown
            .IndexOf(preset => preset.Equals(currentPreset));

        presetDropdown.SetSelectedIndexWithoutNotify(currentpresetIndex);
    }

    private void OnHandPresetRepositoryRefreshed(object sender, EventArgs e)
    {
        if (handPresetRepository.ContainsCategory(presetCategoryDropdown.SelectedItem))
        {
            var currentCategory = presetCategoryDropdown.SelectedItem;
            var newCategories = PresetCategoryList().ToArray();

            handPresetCategoryComboBox.SetItems(newCategories);

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
            handPresetCategoryComboBox.SetItems(newCategories);
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
                .OrderBy(preset => preset.Name, new WindowsLogicalStringComparer());
}
