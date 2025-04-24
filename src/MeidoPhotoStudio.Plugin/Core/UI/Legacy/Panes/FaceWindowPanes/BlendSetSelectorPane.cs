using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BlendSetSelectorPane : BasePane
{
    private readonly Translation translation;
    private readonly GameBlendSetRepository gameBlendSetRepository;
    private readonly CustomBlendSetRepository customBlendSetRepository;
    private readonly FacialExpressionBuilder facialExpressionBuilder;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle.Group blendSetSourceGroup;
    private readonly Dictionary<BlendSetSource, Toggle> blendSetSourceToggles;
    private readonly Dropdown<string> blendSetCategoryDropdown;
    private readonly Dropdown<IBlendSetModel> blendSetDropdown;
    private readonly SubPaneHeader saveBlendSetToggle;
    private readonly Framework.UI.Legacy.ComboBox blendSetCategoryComboBox;
    private readonly TextField blendSetNameTextField;
    private readonly Button saveBlendSetButton;
    private readonly Label noBlendSetsLabel;
    private readonly Header blendSetDirectoryHeader;
    private readonly Header blendSetFilenameHeader;
    private readonly Button refreshButton;
    private readonly Label savedBlendSetLabel;
    private readonly SearchBar<IBlendSetModel> searchBar;
    private readonly LazyStyle noBlendSetsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private BlendSetSource currentBlendSetSource = BlendSetSource.Game;
    private bool showSaveBlendSetLabel;
    private float saveTime;

    public BlendSetSelectorPane(
        Translation translation,
        GameBlendSetRepository gameBlendSetRepository,
        CustomBlendSetRepository customBlendSetRepository,
        FacialExpressionBuilder facialExpressionBuilder,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.gameBlendSetRepository = gameBlendSetRepository ?? throw new ArgumentNullException(nameof(gameBlendSetRepository));
        this.customBlendSetRepository = customBlendSetRepository ?? throw new ArgumentNullException(nameof(customBlendSetRepository));
        this.facialExpressionBuilder = facialExpressionBuilder ?? throw new ArgumentNullException(nameof(facialExpressionBuilder));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.customBlendSetRepository.AddedBlendSet += OnBlendSetAdded;
        this.customBlendSetRepository.Refreshed += OnCustomBlendSetRepositoryRefreshed;
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        this.translation.Initialized += OnTranslationInitialized;

        var gameToggle = new Toggle(new LocalizableGUIContent(translation, "maidFaceWindow", "baseTab"), true);

        gameToggle.ControlEvent += OnBlendSetSourceToggleChanged(BlendSetSource.Game);

        var customToggle = new Toggle(new LocalizableGUIContent(translation, "maidFaceWindow", "customTab"));

        customToggle.ControlEvent += OnBlendSetSourceToggleChanged(BlendSetSource.Custom);

        blendSetSourceToggles = new()
        {
            [BlendSetSource.Game] = gameToggle,
            [BlendSetSource.Custom] = customToggle,
        };

        blendSetSourceGroup = [gameToggle, customToggle];

        searchBar = new(SearchSelector, BlendSetFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "maidFaceWindow", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        blendSetCategoryDropdown = new(
            BlendSetCategoryList(currentBlendSetSource),
            formatter: GetBlendSetCategoryFormatter(currentBlendSetSource));

        blendSetCategoryDropdown.SelectionChanged += OnBlendSetCategoryChanged;

        blendSetDropdown = new(BlendSetList(currentBlendSetSource), formatter: BlendSetFormatter);

        blendSetDropdown.SelectionChanged += OnBlendSetChanged;

        saveBlendSetToggle = new(new LocalizableGUIContent(translation, "maidFaceWindow", "savePaneToggle"), false);

        blendSetCategoryComboBox = new(this.customBlendSetRepository.Categories)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "maidFaceWindow", "categorySearchBarPlaceholder"),
        };

        blendSetNameTextField = new()
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "maidFaceWindow", "nameTextFieldPlaceholder"),
        };

        saveBlendSetButton = new(new LocalizableGUIContent(translation, "maidFaceWindow", "saveButton"));
        saveBlendSetButton.ControlEvent += OnSaveBlendSetButtonPushed;

        refreshButton = new(new LocalizableGUIContent(translation, "maidFaceWindow", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        noBlendSetsLabel = new(new LocalizableGUIContent(translation, "maidFaceWindow", "noBlendSets"));
        blendSetDirectoryHeader = new(new LocalizableGUIContent(translation, "maidFaceWindow", "directoryHeader"));
        blendSetFilenameHeader = new(new LocalizableGUIContent(translation, "maidFaceWindow", "filenameHeader"));

        savedBlendSetLabel = new(new LocalizableGUIContent(translation, "maidFaceWindow", "savedBlendSetLabel"));

        IEnumerable<IBlendSetModel> SearchSelector(string query)
        {
            var repository = currentBlendSetSource is BlendSetSource.Game
                ? gameBlendSetRepository.Cast<IBlendSetModel>()
                : customBlendSetRepository.Cast<IBlendSetModel>();

            return repository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        IDropdownItem BlendSetFormatter(IBlendSetModel blendSet, int index) =>
            new LabelledDropdownItem($"{index + 1}: {blendSet.Name}");

        EventHandler OnBlendSetSourceToggleChanged(BlendSetSource source) =>
            (sender, _) =>
            {
                if (sender is not Toggle { Value: true })
                    return;

                ChangeBlendSetSource(source);
            };

        void OnTranslationInitialized(object sender, EventArgs e)
        {
            if (currentBlendSetSource is BlendSetSource.Game)
                blendSetCategoryDropdown.Reformat();
        }
    }

    private enum BlendSetSource
    {
        Game,
        Custom,
    }

    private FaceController CurrentFace =>
        characterSelectionController.Current?.Face;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        GUILayout.BeginHorizontal();

        foreach (var sourceToggle in blendSetSourceGroup)
            sourceToggle.Draw();

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        GUI.enabled = enabled;

        if (!blendSetCategoryDropdown.Any())
        {
            noBlendSetsLabel.Draw(noBlendSetsLabelStyle);
        }
        else if (!blendSetDropdown.Any())
        {
            DrawTextFieldWithScrollBarOffset(searchBar);

            DrawDropdown(blendSetCategoryDropdown);
            noBlendSetsLabel.Draw(noBlendSetsLabelStyle);
        }
        else
        {
            DrawTextFieldWithScrollBarOffset(searchBar);

            DrawDropdown(blendSetCategoryDropdown);
            DrawDropdown(blendSetDropdown);
        }

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        saveBlendSetToggle.Draw();

        if (currentBlendSetSource is BlendSetSource.Custom)
            refreshButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        if (saveBlendSetToggle.Enabled)
            DrawAddBlendSet();

        void DrawAddBlendSet()
        {
            blendSetDirectoryHeader.Draw();
            DrawComboBox(blendSetCategoryComboBox);

            blendSetFilenameHeader.Draw();
            DrawTextFieldWithScrollBarOffset(blendSetNameTextField);

            UIUtility.DrawBlackLine();

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

    private Func<string, int, IDropdownItem> GetBlendSetCategoryFormatter(BlendSetSource source)
    {
        return source is BlendSetSource.Custom ? CustomBlendSetCategoryFormatter : GameBlendSetCategoryFormatter;

        static LabelledDropdownItem CustomBlendSetCategoryFormatter(string category, int index) =>
            new(category);

        LabelledDropdownItem GameBlendSetCategoryFormatter(string category, int index) =>
            new(translation["faceBlendCategory", category]);
    }

    private void OnBlendSetAdded(object sender, AddedBlendSetEventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (currentBlendSetSource is not BlendSetSource.Custom)
            return;

        var currentCategory = blendSetCategoryDropdown.SelectedItem;

        if (!blendSetCategoryDropdown.Contains(e.BlendSet.Category))
        {
            blendSetCategoryDropdown.SetItemsWithoutNotify(BlendSetCategoryList(BlendSetSource.Custom));
            blendSetCategoryComboBox.SetItems(customBlendSetRepository.Categories);
        }

        var currentCategoryIndex = blendSetCategoryDropdown.IndexOf(currentCategory, StringComparer.Ordinal);

        blendSetCategoryDropdown.SetSelectedIndexWithoutNotify(currentCategoryIndex);

        if (!string.Equals(e.BlendSet.Category, currentCategory, StringComparison.Ordinal))
            return;

        var currentBlendSet = blendSetDropdown.SelectedItem;

        blendSetDropdown.SetItemsWithoutNotify(BlendSetList(BlendSetSource.Custom));

        var currentBlendSetIndex = blendSetDropdown.IndexOf(currentBlendSet);

        blendSetDropdown.SetSelectedIndexWithoutNotify(currentBlendSetIndex);
    }

    private void OnCustomBlendSetRepositoryRefreshed(object sender, EventArgs e)
    {
        var newCategories = BlendSetCategoryList(BlendSetSource.Custom).ToArray();

        blendSetCategoryComboBox.SetItems(newCategories);

        if (currentBlendSetSource is not BlendSetSource.Custom)
            return;

        if (CurrentFace?.BlendSet is not CustomBlendSetModel blendSet)
            return;

        if (customBlendSetRepository.ContainsCategory(blendSet.Category))
        {
            var currentCategory = blendSet.Category;
            var categoryIndex = newCategories.IndexOf(currentCategory, StringComparer.Ordinal);

            if (categoryIndex < 0)
            {
                blendSetCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
                blendSetDropdown.SetItemsWithoutNotify(BlendSetList(BlendSetSource.Custom), 0);

                return;
            }

            blendSetCategoryDropdown.SetItemsWithoutNotify(newCategories, categoryIndex);

            var newblendSets = BlendSetList(BlendSetSource.Custom).Cast<CustomBlendSetModel>().ToArray();
            var blendSetIndex = newblendSets.FindIndex(newBlendSet => blendSet.ID == newBlendSet.ID);

            if (blendSetIndex < 0)
                blendSetIndex = 0;

            blendSetDropdown.SetItemsWithoutNotify(newblendSets, blendSetIndex);
        }
        else
        {
            blendSetCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
            blendSetDropdown.SetItemsWithoutNotify(BlendSetList(BlendSetSource.Custom), 0);
        }
    }

    private void ChangeBlendSetSource(BlendSetSource source)
    {
        currentBlendSetSource = source;
        blendSetCategoryDropdown.SetItemsWithoutNotify(BlendSetCategoryList(currentBlendSetSource), 0);
        blendSetCategoryDropdown.Formatter = GetBlendSetCategoryFormatter(currentBlendSetSource);

        blendSetDropdown.SetItemsWithoutNotify(BlendSetList(currentBlendSetSource), 0);

        if (blendSetDropdown.SelectedItem is not null)
            CurrentFace?.ApplyBlendSet(blendSetDropdown.SelectedItem);

        var query = searchBar.Query;

        searchBar.ClearQuery();
        searchBar.SetQueryWithoutShowingResults(query);
    }

    private void OnBlendSetCategoryChanged(object sender, DropdownEventArgs<string> e)
    {
        if (e.PreviousSelectedItemIndex == e.SelectedItemIndex)
            blendSetDropdown.SelectedItemIndex = 0;
        else
            blendSetDropdown.SetItems(BlendSetList(currentBlendSetSource), 0);
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
        var blendSetSource = blendSet.Custom ? BlendSetSource.Custom : BlendSetSource.Game;

        if (blendSetSource != currentBlendSetSource)
        {
            currentBlendSetSource = blendSetSource;
            blendSetSourceToggles[blendSetSource].SetEnabledWithoutNotify(true);

            blendSetCategoryDropdown.SetItemsWithoutNotify(BlendSetCategoryList(blendSetSource));
            blendSetCategoryDropdown.Formatter = GetBlendSetCategoryFormatter(blendSetSource);

            var categoryIndex = GetCategoryIndex(blendSet);

            blendSetCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            var newBlendSets = BlendSetList(blendSetSource);
            var blendSetIndex = GetBlendSetIndex(newBlendSets, blendSet);

            blendSetDropdown.SetItemsWithoutNotify(newBlendSets, blendSetIndex);
        }
        else
        {
            var categoryIndex = GetCategoryIndex(blendSet);
            var oldCategoryIndex = blendSetCategoryDropdown.SelectedItemIndex;

            blendSetCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                blendSetDropdown.SetItemsWithoutNotify(BlendSetList(blendSetSource));

            var blendSetIndex = GetBlendSetIndex(blendSetDropdown, blendSet);

            blendSetDropdown.SetSelectedIndexWithoutNotify(blendSetIndex);
        }

        int GetCategoryIndex(IBlendSetModel blendSet)
        {
            var categoryIndex = blendSetCategoryDropdown.IndexOf(blendSet.Category, StringComparer.Ordinal);

            return categoryIndex < 0 ? 0 : categoryIndex;
        }

        int GetBlendSetIndex(IEnumerable<IBlendSetModel> blendSetList, IBlendSetModel blendSetToFind) =>
            blendSetToFind.Custom
                ? customBlendSetRepository.ContainsCategory(blendSetToFind.Category)
                    ? blendSetList.IndexOf(blendSetToFind)
                    : 0
                : gameBlendSetRepository.ContainsCategory(blendSetToFind.Category)
                    ? blendSetList.IndexOf(blendSetToFind)
                    : 0;
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

    private IEnumerable<string> BlendSetCategoryList(BlendSetSource source) =>
        source switch
        {
            BlendSetSource.Game => gameBlendSetRepository.Categories,
            BlendSetSource.Custom => customBlendSetRepository.Categories
                .OrderBy(category => !string.Equals(category, customBlendSetRepository.RootCategoryName, StringComparison.Ordinal))
                .ThenBy(category => category, new WindowsLogicalStringComparer()),
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };

    private IEnumerable<IBlendSetModel> BlendSetList(BlendSetSource source)
    {
        return source is BlendSetSource.Custom ? GetCustomBlendSets() : GetGameBlendSets();

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
