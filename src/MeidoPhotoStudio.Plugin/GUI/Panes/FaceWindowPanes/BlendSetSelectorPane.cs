using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class BlendSetSelectorPane : BasePane
{
    private const int GameBlendSet = 0;
    private const int CustomBlendSet = 1;

    private static readonly string[] BlendSetSourceTranslationKeys = ["baseTab", "customTab"];

    private readonly GameBlendSetRepository gameBlendSetRepository;
    private readonly CustomBlendSetRepository customBlendSetRepository;
    private readonly FacialExpressionBuilder facialExpressionBuilder;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle paneHeader;
    private readonly SelectionGrid blendSetSourceGrid;
    private readonly Dropdown2<string> blendSetCategoryDropdown;
    private readonly Dropdown2<IBlendSetModel> blendSetDropdown;
    private readonly Toggle saveBlendSetToggle;
    private readonly ComboBox blendSetCategoryComboBox;
    private readonly TextField blendSetNameTextField;
    private readonly Button saveBlendSetButton;
    private readonly Label noBlendSetsLabel;
    private readonly Header blendSetDirectoryHeader;
    private readonly Header blendSetFilenameHeader;
    private readonly Button refreshButton;

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
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        var sourceIndex = GameBlendSet;

        blendSetSourceGrid = new(Translation.GetArray("maidFaceWindow", BlendSetSourceTranslationKeys), sourceIndex);
        blendSetSourceGrid.ControlEvent += OnBlendSetSourceChanged;

        blendSetCategoryDropdown = new(
            BlendSetCategoryList(sourceIndex is CustomBlendSet),
            formatter: GetBlendSetCategoryFormatter(sourceIndex is CustomBlendSet));

        blendSetCategoryDropdown.SelectionChanged += OnBlendSetCategoryChanged;

        blendSetDropdown = new(
            BlendSetList(sourceIndex is CustomBlendSet),
            formatter: (blendSet, index) => $"{index + 1}: {blendSet.Name}");

        blendSetDropdown.SelectionChanged += OnBlendSetChanged;

        paneHeader = new("Face Blend Sets", true);

        saveBlendSetToggle = new("Save Blend Set", false);
        blendSetCategoryComboBox = new(this.customBlendSetRepository.Categories.ToArray());
        blendSetNameTextField = new();
        saveBlendSetButton = new("Save");
        saveBlendSetButton.ControlEvent += OnSaveBlendSetButtonPushed;

        refreshButton = new("Refresh");
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        noBlendSetsLabel = new(Translation.Get("maidFaceWindow", "noBlendSets"));
        blendSetDirectoryHeader = new(Translation.Get("maidFaceWindow", "directoryHeader"));
        blendSetFilenameHeader = new(Translation.Get("maidFaceWindow", "filenameHeader"));
    }

    private FaceController CurrentFace =>
        characterSelectionController.Current?.Face;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
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
            DrawDropdown(blendSetCategoryDropdown);
            noBlendSetsLabel.Draw();
        }
        else
        {
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
            var parentWidth = parent.WindowRect.width;
            var width = GUILayout.Width(parentWidth - 75f);

            blendSetDirectoryHeader.Draw();
            blendSetCategoryComboBox.Draw(width);

            blendSetFilenameHeader.Draw();
            GUILayout.BeginHorizontal();

            blendSetNameTextField.Draw(width);

            saveBlendSetButton.Draw(GUILayout.ExpandWidth(false));

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
        blendSetSourceGrid.SetItemsWithoutNotify(Translation.GetArray("maidFaceWindow", BlendSetSourceTranslationKeys));

        if (blendSetSourceGrid.SelectedItemIndex is GameBlendSet)
            blendSetCategoryDropdown.Reformat();

        paneHeader.Label = Translation.Get("maidFaceWindow", "header");
        saveBlendSetToggle.Label = Translation.Get("maidFaceWindow", "savePaneToggle");
        saveBlendSetButton.Label = Translation.Get("maidFaceWindow", "saveButton");
        noBlendSetsLabel.Text = Translation.Get("maidFaceWindow", "noBlendSets");
        blendSetDirectoryHeader.Text = Translation.Get("maidFaceWindow", "directoryHeader");
        blendSetFilenameHeader.Text = Translation.Get("maidFaceWindow", "filenameHeader");
    }

    private static Func<string, int, string> GetBlendSetCategoryFormatter(bool custom)
    {
        return custom ? CustomBlendSetCategoryFormatter : GameBlendSetCategoryFormatter;

        static string CustomBlendSetCategoryFormatter(string category, int index) =>
            category;

        static string GameBlendSetCategoryFormatter(string category, int index) =>
            Translation.Get("faceBlendCategory", category);
    }

    private void OnBlendSetAdded(object sender, AddedBlendSetEventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (blendSetSourceGrid.SelectedItemIndex is CustomBlendSet)
        {
            if (!blendSetCategoryDropdown.Contains(e.BlendSet.Category))
            {
                blendSetCategoryDropdown.SetItemsWithoutNotify(BlendSetCategoryList(e.BlendSet.Custom));
                blendSetCategoryComboBox.BaseDropDown.SetDropdownItemsWithoutNotify([.. customBlendSetRepository.Categories], 0);
            }

            blendSetDropdown.SetItemsWithoutNotify(BlendSetList(e.BlendSet.Custom));
        }

        CurrentFace.ApplyBlendSet(e.BlendSet);

        UpdatePanel(e.BlendSet);
    }

    private void OnCustomBlendSetRepositoryRefreshed(object sender, EventArgs e)
    {
        if (blendSetSourceGrid.SelectedItemIndex is not CustomBlendSet)
            return;

        if (customBlendSetRepository.ContainsCategory(blendSetCategoryDropdown.SelectedItem))
        {
            var currentCategory = blendSetCategoryDropdown.SelectedItem;
            var newCategories = BlendSetCategoryList(custom: true).ToArray();

            blendSetCategoryComboBox.SetDropdownItems(newCategories);

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
            blendSetCategoryComboBox.BaseDropDown.SetDropdownItemsWithoutNotify(newCategories);
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
    }

    private void OnBlendSetCategoryChanged(object sender, DropdownEventArgs<string> e)
    {
        if (e.PreviousSelectedItemIndex == e.SelectedItemIndex)
            blendSetDropdown.SelectedItemIndex = 0;
        else
            blendSetDropdown.SetItems(BlendSetList(blendSetSourceGrid.SelectedItemIndex is CustomBlendSet), 0);
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

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

            var blendSetIndex = GetBlendSetIndex(blendSet);

            blendSetDropdown.SetItemsWithoutNotify(BlendSetList(blendSet.Custom), blendSetIndex);
        }
        else
        {
            var categoryIndex = GetCategoryIndex(blendSet);
            var oldCategoryIndex = blendSetCategoryDropdown.SelectedItemIndex;

            blendSetCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                blendSetDropdown.SetItemsWithoutNotify(BlendSetList(blendSet.Custom));

            var blendSetIndex = GetBlendSetIndex(blendSet);

            blendSetDropdown.SetSelectedIndexWithoutNotify(blendSetIndex);
        }

        int GetCategoryIndex(IBlendSetModel blendSet)
        {
            var categoryIndex = blendSetCategoryDropdown.IndexOf(category =>
                string.Equals(category, blendSet.Category, StringComparison.OrdinalIgnoreCase));

            return categoryIndex < 0 ? 0 : categoryIndex;
        }

        int GetBlendSetIndex(IBlendSetModel blendSetToFind)
        {
            if (blendSetToFind.Custom)
            {
                var customBlendSet = (CustomBlendSetModel)blendSetToFind;

                return customBlendSetRepository.ContainsCategory(blendSetToFind.Category)
                    ? blendSetDropdown
                        .Cast<CustomBlendSetModel>()
                        .IndexOf(blendSet => blendSet.ID == customBlendSet.ID)
                    : 0;
            }
            else
            {
                var gameBlendSet = (GameBlendSetModel)blendSetToFind;

                return gameBlendSetRepository.ContainsCategory(blendSetToFind.Category)
                    ? gameBlendSetRepository
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
