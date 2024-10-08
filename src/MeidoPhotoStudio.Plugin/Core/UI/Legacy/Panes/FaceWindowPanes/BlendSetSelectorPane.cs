using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BlendSetSelectorPane : BasePane
{
    private const int GameBlendSet = 0;
    private const int CustomBlendSet = 1;

    private static readonly string[] BlendSetSourceTranslationKeys = ["baseTab", "customTab"];

    private readonly GameBlendSetRepository gameBlendSetRepository;
    private readonly CustomBlendSetRepository customBlendSetRepository;
    private readonly FacialExpressionBuilder facialExpressionBuilder;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly PaneHeader paneHeader;
    private readonly SelectionGrid blendSetSourceGrid;
    private readonly Dropdown<string> blendSetCategoryDropdown;
    private readonly Dropdown<IBlendSetModel> blendSetDropdown;
    private readonly Toggle saveBlendSetToggle;
    private readonly Framework.UI.Legacy.ComboBox blendSetCategoryComboBox;
    private readonly TextField blendSetNameTextField;
    private readonly Button saveBlendSetButton;
    private readonly Label noBlendSetsLabel;
    private readonly Header blendSetDirectoryHeader;
    private readonly Header blendSetFilenameHeader;
    private readonly Button refreshButton;
    private readonly Label savedBlendSetLabel;
    private readonly SearchBar<IBlendSetModel> searchBar;

    private bool showSaveBlendSetLabel;
    private float saveTime;

    public BlendSetSelectorPane(
        GameBlendSetRepository gameBlendSetRepository,
        CustomBlendSetRepository customBlendSetRepository,
        FacialExpressionBuilder facialExpressionBuilder,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.gameBlendSetRepository = gameBlendSetRepository ?? throw new ArgumentNullException(nameof(gameBlendSetRepository));
        this.customBlendSetRepository = customBlendSetRepository ?? throw new ArgumentNullException(nameof(customBlendSetRepository));
        this.facialExpressionBuilder = facialExpressionBuilder ?? throw new ArgumentNullException(nameof(facialExpressionBuilder));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.customBlendSetRepository.AddedBlendSet += OnBlendSetAdded;
        this.customBlendSetRepository.Refreshed += OnCustomBlendSetRepositoryRefreshed;
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        var sourceIndex = GameBlendSet;

        blendSetSourceGrid = new(Translation.GetArray("maidFaceWindow", BlendSetSourceTranslationKeys), sourceIndex);
        blendSetSourceGrid.ControlEvent += OnBlendSetSourceChanged;

        searchBar = new(SearchSelector, PropFormatter)
        {
            Placeholder = Translation.Get("maidFaceWindow", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        blendSetCategoryDropdown = new(
            BlendSetCategoryList(sourceIndex is CustomBlendSet),
            formatter: GetBlendSetCategoryFormatter(sourceIndex is CustomBlendSet));

        blendSetCategoryDropdown.SelectionChanged += OnBlendSetCategoryChanged;

        blendSetDropdown = new(BlendSetList(sourceIndex is CustomBlendSet), formatter: PropFormatter);

        blendSetDropdown.SelectionChanged += OnBlendSetChanged;

        paneHeader = new(Translation.Get("maidFaceWindow", "header"), true);

        saveBlendSetToggle = new(Translation.Get("maidFaceWindow", "savePaneToggle"), false);
        blendSetCategoryComboBox = new(this.customBlendSetRepository.Categories);
        blendSetNameTextField = new();
        saveBlendSetButton = new(Translation.Get("maidFaceWindow", "saveButton"));
        saveBlendSetButton.ControlEvent += OnSaveBlendSetButtonPushed;

        refreshButton = new(Translation.Get("maidFaceWindow", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        noBlendSetsLabel = new(Translation.Get("maidFaceWindow", "noBlendSets"));
        blendSetDirectoryHeader = new(Translation.Get("maidFaceWindow", "directoryHeader"));
        blendSetFilenameHeader = new(Translation.Get("maidFaceWindow", "filenameHeader"));

        savedBlendSetLabel = new(Translation.Get("maidFaceWindow", "savedBlendSetLabel"));

        IEnumerable<IBlendSetModel> SearchSelector(string query)
        {
            var repository = blendSetSourceGrid.SelectedItemIndex is GameBlendSet
                ? gameBlendSetRepository.Cast<IBlendSetModel>()
                : customBlendSetRepository.Cast<IBlendSetModel>();

            return repository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        IDropdownItem PropFormatter(IBlendSetModel blendSet, int index) =>
            new LabelledDropdownItem($"{index + 1}: {blendSet.Name}");
    }

    private FaceController CurrentFace =>
        characterSelectionController.Current?.Face;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        blendSetSourceGrid.Draw();
        MpsGui.BlackLine();

        GUI.enabled = enabled;

        if (!blendSetCategoryDropdown.Any())
        {
            noBlendSetsLabel.Draw();
        }
        else if (!blendSetDropdown.Any())
        {
            searchBar.Draw();

            DrawDropdown(blendSetCategoryDropdown);
            noBlendSetsLabel.Draw();
        }
        else
        {
            searchBar.Draw();

            DrawDropdown(blendSetCategoryDropdown);
            DrawDropdown(blendSetDropdown);
        }

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        saveBlendSetToggle.Draw();

        if (blendSetSourceGrid.SelectedItemIndex is CustomBlendSet)
        {
            GUILayout.FlexibleSpace();

            refreshButton.Draw();
        }

        GUILayout.EndHorizontal();

        if (saveBlendSetToggle.Value)
            DrawAddBlendSet();

        void DrawAddBlendSet()
        {
            blendSetDirectoryHeader.Draw();
            DrawComboBox(blendSetCategoryComboBox);

            blendSetFilenameHeader.Draw();
            DrawTextFieldMaxWidth(blendSetNameTextField);

            MpsGui.BlackLine();

            saveBlendSetButton.Draw();

            if (!showSaveBlendSetLabel)
                return;

            if (Time.time - saveTime >= 2.5f)
            {
                showSaveBlendSetLabel = false;

                return;
            }

            savedBlendSetLabel.Draw();
        }
    }

    protected override void ReloadTranslation()
    {
        blendSetSourceGrid.SetItemsWithoutNotify(Translation.GetArray("maidFaceWindow", BlendSetSourceTranslationKeys));
        if (blendSetSourceGrid.SelectedItemIndex is GameBlendSet)
            blendSetCategoryDropdown.Reformat();

        paneHeader.Label = Translation.Get("maidFaceWindow", "header");
        saveBlendSetToggle.Label = Translation.Get("maidFaceWindow", "savePaneToggle");
        saveBlendSetButton.Label = Translation.Get("maidFaceWindow", "saveButton");
        refreshButton.Label = Translation.Get("maidFaceWindow", "refreshButton");
        noBlendSetsLabel.Text = Translation.Get("maidFaceWindow", "noBlendSets");
        blendSetDirectoryHeader.Text = Translation.Get("maidFaceWindow", "directoryHeader");
        blendSetFilenameHeader.Text = Translation.Get("maidFaceWindow", "filenameHeader");
        savedBlendSetLabel.Text = Translation.Get("maidFaceWindow", "savedBlendSetLabel");
        searchBar.Placeholder = Translation.Get("maidFaceWindow", "searchBarPlaceholder");
    }

    private static Func<string, int, IDropdownItem> GetBlendSetCategoryFormatter(bool custom)
    {
        return custom ? CustomBlendSetCategoryFormatter : GameBlendSetCategoryFormatter;

        static LabelledDropdownItem CustomBlendSetCategoryFormatter(string category, int index) =>
            new(category);

        static LabelledDropdownItem GameBlendSetCategoryFormatter(string category, int index) =>
            new(Translation.Get("faceBlendCategory", category));
    }

    private void OnBlendSetAdded(object sender, AddedBlendSetEventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (blendSetSourceGrid.SelectedItemIndex is not CustomBlendSet)
            return;

        var currentCategory = blendSetCategoryDropdown.SelectedItem;

        if (!blendSetCategoryDropdown.Contains(e.BlendSet.Category))
        {
            blendSetCategoryDropdown.SetItemsWithoutNotify(BlendSetCategoryList(e.BlendSet.Custom));
            blendSetCategoryComboBox.SetItems(customBlendSetRepository.Categories);
        }

        var currentCategoryIndex = blendSetCategoryDropdown
            .IndexOf(category => string.Equals(category, currentCategory, StringComparison.Ordinal));

        blendSetCategoryDropdown.SetSelectedIndexWithoutNotify(currentCategoryIndex);

        if (!string.Equals(e.BlendSet.Category, currentCategory, StringComparison.Ordinal))
            return;

        var currentBlendSet = blendSetDropdown.SelectedItem;

        blendSetDropdown.SetItemsWithoutNotify(BlendSetList(e.BlendSet.Custom));

        var currentBlendSetIndex = blendSetDropdown
            .IndexOf(blendSet => blendSet.Equals(currentBlendSet));

        blendSetDropdown.SetSelectedIndexWithoutNotify(currentBlendSetIndex);
    }

    private void OnCustomBlendSetRepositoryRefreshed(object sender, EventArgs e)
    {
        if (blendSetSourceGrid.SelectedItemIndex is not CustomBlendSet)
            return;

        if (customBlendSetRepository.ContainsCategory(blendSetCategoryDropdown.SelectedItem))
        {
            var currentCategory = blendSetCategoryDropdown.SelectedItem;
            var newCategories = BlendSetCategoryList(custom: true).ToArray();

            blendSetCategoryComboBox.SetItems(newCategories);

            var categoryIndex = newCategories.IndexOf(category => string.Equals(currentCategory, category, StringComparison.Ordinal));

            if (categoryIndex < 0)
            {
                blendSetCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
                blendSetDropdown.SetItemsWithoutNotify(BlendSetList(custom: true), 0);

                return;
            }

            blendSetCategoryDropdown.SetItemsWithoutNotify(newCategories, categoryIndex);

            var currentblendSetModel = blendSetDropdown.SelectedItem;

            var newblendSets = BlendSetList(custom: true).ToArray();
            var blendSetIndex = newblendSets.IndexOf(blendSet => blendSet == currentblendSetModel);

            if (blendSetIndex < 0)
                blendSetIndex = 0;

            blendSetDropdown.SetItemsWithoutNotify(newblendSets, blendSetIndex);
        }
        else
        {
            var newCategories = BlendSetCategoryList(custom: true).ToArray();

            blendSetCategoryDropdown.SetItems(newCategories, 0);
            blendSetCategoryComboBox.SetItems(newCategories);
        }
    }

    private void OnBlendSetSourceChanged(object sender, EventArgs e)
    {
        var custom = blendSetSourceGrid.SelectedItemIndex is CustomBlendSet;

        blendSetCategoryDropdown.SetItemsWithoutNotify(BlendSetCategoryList(custom), 0);
        blendSetCategoryDropdown.Formatter = GetBlendSetCategoryFormatter(custom);

        blendSetDropdown.SetItemsWithoutNotify(BlendSetList(custom), 0);

        if (blendSetDropdown.SelectedItem is not null)
            CurrentFace?.ApplyBlendSet(blendSetDropdown.SelectedItem);

        searchBar.ClearQuery();
    }

    private void OnBlendSetCategoryChanged(object sender, DropdownEventArgs<string> e)
    {
        if (e.PreviousSelectedItemIndex == e.SelectedItemIndex)
            blendSetDropdown.SelectedItemIndex = 0;
        else
            blendSetDropdown.SetItems(BlendSetList(blendSetSourceGrid.SelectedItemIndex is CustomBlendSet), 0);
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Face.PropertyChanged -= OnFacePropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Face.PropertyChanged += OnFacePropertyChanged;

        UpdatePanel(CurrentFace.BlendSet);
    }

    private void OnSaveBlendSetButtonPushed(object sender, EventArgs e)
    {
        if (CurrentFace is null)
            return;

        var category = blendSetCategoryComboBox.Value;
        var name = blendSetNameTextField.Value;

        if (string.IsNullOrEmpty(category))
            category = customBlendSetRepository.RootCategoryName;

        if (string.IsNullOrEmpty(name))
            name = "face_preset";

        customBlendSetRepository.Add(facialExpressionBuilder.Build(CurrentFace), category, name);

        blendSetNameTextField.Value = string.Empty;

        showSaveBlendSetLabel = true;
        saveTime = Time.time;
    }

    private void OnRefreshButtonPushed(object sender, EventArgs e) =>
        customBlendSetRepository.Refresh();

    private void UpdatePanel(IBlendSetModel blendSet)
    {
        var blendSetSource = blendSet.Custom ? CustomBlendSet : GameBlendSet;

        if (blendSetSource != blendSetSourceGrid.SelectedItemIndex)
        {
            blendSetSourceGrid.SetValueWithoutNotify(blendSetSource);

            var categoryIndex = GetCategoryIndex(blendSet);
            var custom = blendSetSourceGrid.SelectedItemIndex is CustomBlendSet;

            blendSetCategoryDropdown.SetItemsWithoutNotify(BlendSetCategoryList(blendSet.Custom), categoryIndex);
            blendSetCategoryDropdown.Formatter = GetBlendSetCategoryFormatter(custom);

            var newBlendSets = BlendSetList(blendSet.Custom);
            var blendSetIndex = GetBlendSetIndex(newBlendSets, blendSet);

            blendSetDropdown.SetItemsWithoutNotify(newBlendSets, blendSetIndex);
        }
        else
        {
            var categoryIndex = GetCategoryIndex(blendSet);
            var oldCategoryIndex = blendSetCategoryDropdown.SelectedItemIndex;

            blendSetCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                blendSetDropdown.SetItemsWithoutNotify(BlendSetList(blendSet.Custom));

            var blendSetIndex = GetBlendSetIndex(blendSetDropdown, blendSet);

            blendSetDropdown.SetSelectedIndexWithoutNotify(blendSetIndex);
        }

        int GetCategoryIndex(IBlendSetModel blendSet)
        {
            var categoryIndex = blendSetCategoryDropdown.IndexOf(category =>
                string.Equals(category, blendSet.Category, StringComparison.OrdinalIgnoreCase));

            return categoryIndex < 0 ? 0 : categoryIndex;
        }

        int GetBlendSetIndex(IEnumerable<IBlendSetModel> blendSetList, IBlendSetModel blendSetToFind)
        {
            if (blendSetToFind.Custom)
            {
                var customBlendSet = (CustomBlendSetModel)blendSetToFind;

                return customBlendSetRepository.ContainsCategory(blendSetToFind.Category)
                    ? blendSetList
                        .Cast<CustomBlendSetModel>()
                        .IndexOf(blendSet => blendSet.ID == customBlendSet.ID)
                    : 0;
            }
            else
            {
                var gameBlendSet = (GameBlendSetModel)blendSetToFind;

                return gameBlendSetRepository.ContainsCategory(blendSetToFind.Category)
                    ? blendSetList
                        .Cast<GameBlendSetModel>()
                        .IndexOf(blendSet => blendSet.ID == gameBlendSet.ID)
                    : 0;
            }
        }
    }

    private void OnBlendSetChanged(object sender, DropdownEventArgs<IBlendSetModel> e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (e.Item is null)
            return;

        CurrentFace?.ApplyBlendSet(e.Item);
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<IBlendSetModel> e)
    {
        if (e.Item is null)
            return;

        CurrentFace?.ApplyBlendSet(e.Item);
    }

    private void OnFacePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(FaceController.BlendSet))
            return;

        var controller = (FaceController)sender;

        if (controller.BlendSet.Equals(blendSetDropdown.SelectedItem))
            return;

        UpdatePanel(controller.BlendSet);
    }

    private IEnumerable<string> BlendSetCategoryList(bool custom) =>
        custom ? customBlendSetRepository.Categories
            .OrderBy(category => !string.Equals(category, customBlendSetRepository.RootCategoryName, StringComparison.Ordinal))
            .ThenBy(category => category, new WindowsLogicalStringComparer()) :
        gameBlendSetRepository.Categories;

    private IEnumerable<IBlendSetModel> BlendSetList(bool custom)
    {
        return custom ? GetCustomBlendSets() : GetGameBlendSets();

        IEnumerable<IBlendSetModel> GetGameBlendSets() =>
            blendSetCategoryDropdown.Any()
                ? gameBlendSetRepository[blendSetCategoryDropdown.SelectedItem]
                    .Cast<IBlendSetModel>()
                : [];

        IEnumerable<IBlendSetModel> GetCustomBlendSets() =>
            blendSetCategoryDropdown.Any()
                ? customBlendSetRepository[blendSetCategoryDropdown.SelectedItem]
                    .OrderBy(blendSet => blendSet.Name, new WindowsLogicalStringComparer())
                    .Cast<IBlendSetModel>()
                : [];
    }
}
