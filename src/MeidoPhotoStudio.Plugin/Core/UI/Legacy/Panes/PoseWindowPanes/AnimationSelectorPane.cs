using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class AnimationSelectorPane : BasePane
{
    private readonly Translation translation;
    private readonly GameAnimationRepository gameAnimationRepository;
    private readonly CustomAnimationRepository customAnimationRepository;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly CustomAnimationRepositorySorter customAnimationRepositorySorter;
    private readonly Toggle.Group animationSourceGroup;
    private readonly Dictionary<AnimationSource, Toggle> animationSourceToggles;
    private readonly Dropdown<string> animationCategoryDropdown;
    private readonly Dropdown<IAnimationModel> animationDropdown;
    private readonly Framework.UI.Legacy.ComboBox animationCategoryComboBox;
    private readonly TextField animationNameTextField;
    private readonly SubPaneHeader saveAnimationToggle;
    private readonly Button savePoseButton;
    private readonly Label initializingLabel;
    private readonly Label noAnimationsLabel;
    private readonly Header animationDirectoryHeader;
    private readonly Header animationFilenameHeader;
    private readonly Button refreshButton;
    private readonly Label savedAnimationLabel;
    private readonly SearchBar<IAnimationModel> searchBar;
    private readonly LazyStyle noAnimationsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private AnimationSource currentAnimationSource = AnimationSource.Game;
    private bool showSavedAnimationLabel;
    private float saveTime;

    public AnimationSelectorPane(
        Translation translation,
        GameAnimationRepository gameAnimationRepository,
        CustomAnimationRepository customAnimationRepository,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController,
        CustomAnimationRepositorySorter customAnimationRepositorySorter)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.gameAnimationRepository = gameAnimationRepository ?? throw new ArgumentNullException(nameof(gameAnimationRepository));
        this.customAnimationRepository = customAnimationRepository ?? throw new ArgumentNullException(nameof(customAnimationRepository));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.customAnimationRepositorySorter = customAnimationRepositorySorter ?? throw new ArgumentNullException(nameof(customAnimationRepositorySorter));

        this.customAnimationRepository.AddedAnimation += OnAnimationAdded;
        this.customAnimationRepository.Refreshed += OnCustomAnimationRepositoryRefreshed;
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        this.translation.Initialized += OnTranslationInitialized;

        var gameToggle = new Toggle(new LocalizableGUIContent(translation, "posePane", "baseTab"), true);

        gameToggle.ControlEvent += OnAnimationSourceToggleChanged(AnimationSource.Game);

        var customToggle = new Toggle(new LocalizableGUIContent(translation, "posePane", "customTab"));

        customToggle.ControlEvent += OnAnimationSourceToggleChanged(AnimationSource.Custom);

        animationSourceGroup = [gameToggle, customToggle];
        animationSourceToggles = new()
        {
            [AnimationSource.Game] = gameToggle,
            [AnimationSource.Custom] = customToggle,
        };

        searchBar = new(SearchSelector, Formatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "posePane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        animationCategoryDropdown = new(GetAnimationCategoryFormatter(currentAnimationSource));
        animationCategoryDropdown.SelectionChanged += OnAnimationCategoryChanged;

        animationDropdown = new(AnimationList(currentAnimationSource), formatter: Formatter);
        animationDropdown.SelectionChanged += OnAnimationChanged;

        saveAnimationToggle = new(new LocalizableGUIContent(translation, "posePane", "saveToggle"), false);

        animationCategoryComboBox = new(this.customAnimationRepository.Categories)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "posePane", "categorySearchBarPlaceholder"),
        };

        animationNameTextField = new()
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "posePane", "nameTextFieldPlaceholder"),
        };

        savePoseButton = new(new LocalizableGUIContent(translation, "posePane", "saveButton"));
        savePoseButton.ControlEvent += OnSavePoseButtonPushed;

        refreshButton = new(new LocalizableGUIContent(translation, "posePane", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        animationDirectoryHeader = new(new LocalizableGUIContent(translation, "posePane", "categoryHeader"));
        animationFilenameHeader = new(new LocalizableGUIContent(translation, "posePane", "nameHeader"));

        initializingLabel = new(new LocalizableGUIContent(translation, "systemMessage", "initializing"));
        noAnimationsLabel = new(new LocalizableGUIContent(translation, "posePane", "noAnimations"));

        savedAnimationLabel = new(new LocalizableGUIContent(translation, "posePane", "savedAnimationLabel"));

        if (gameAnimationRepository.Busy)
            gameAnimationRepository.InitializedAnimations += OnGameAnimationsRepositoryReady;
        else
            InitializeGameAnimations();

        void OnGameAnimationsRepositoryReady(object sender, EventArgs e)
        {
            InitializeGameAnimations();

            gameAnimationRepository.InitializedAnimations -= OnGameAnimationsRepositoryReady;
        }

        void InitializeGameAnimations()
        {
            if (currentAnimationSource is AnimationSource.Custom)
                return;

            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(AnimationSource.Game), 0);
            animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(AnimationSource.Game);

            animationDropdown.SetItemsWithoutNotify(AnimationList(AnimationSource.Game), 0);
        }

        IDropdownItem Formatter(IAnimationModel model, int index) =>
            new LabelledDropdownItem($"{index + 1}: {model.Name}");

        IEnumerable<IAnimationModel> SearchSelector(string query)
        {
            var repository = currentAnimationSource is AnimationSource.Game
                ? gameAnimationRepository.Cast<IAnimationModel>()
                : customAnimationRepository.Cast<IAnimationModel>();

            return repository.Where((IAnimationModel model) =>
            {
                var search = model.Custom ? model.Name : model.Filename;

                return search.Contains(query, StringComparison.OrdinalIgnoreCase);
            });
        }

        EventHandler OnAnimationSourceToggleChanged(AnimationSource source) =>
            (sender, _) =>
            {
                if (sender is not Toggle { Value: true })
                    return;

                ChangeAnimationSource(source);
            };

        void OnTranslationInitialized(object sender, EventArgs e)
        {
            if (currentAnimationSource is AnimationSource.Game)
                animationCategoryDropdown.Reformat();

            searchBar.Reformat();
        }
    }

    private enum AnimationSource
    {
        Game,
        Custom,
    }

    private CharacterController Character =>
        characterSelectionController.Current;

    private AnimationController CurrentAnimation =>
        characterSelectionController.Current?.Animation;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        GUILayout.BeginHorizontal();

        foreach (var sourceToggle in animationSourceGroup)
            sourceToggle.Draw();

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        DrawTextFieldWithScrollBarOffset(searchBar);

        GUI.enabled = enabled;

        if (currentAnimationSource is AnimationSource.Game && gameAnimationRepository.Busy)
            initializingLabel.Draw();
        else
            DrawDropdowns();

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        saveAnimationToggle.Draw();

        if (currentAnimationSource is AnimationSource.Custom)
            refreshButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        if (saveAnimationToggle.Enabled)
            DrawAddAnimation();

        void DrawDropdowns()
        {
            if (!animationCategoryDropdown.Any())
            {
                noAnimationsLabel.Draw(noAnimationsLabelStyle);
            }
            else if (!animationDropdown.Any())
            {
                DrawDropdown(animationCategoryDropdown);

                noAnimationsLabel.Draw(noAnimationsLabelStyle);
            }
            else
            {
                DrawDropdown(animationCategoryDropdown);
                DrawDropdown(animationDropdown);
            }
        }

        void DrawAddAnimation()
        {
            animationDirectoryHeader.Draw();
            DrawComboBox(animationCategoryComboBox);

            animationFilenameHeader.Draw();
            DrawTextFieldWithScrollBarOffset(animationNameTextField);

            UIUtility.DrawBlackLine();

            savePoseButton.Draw();

            if (!showSavedAnimationLabel)
                return;

            if (Time.time - saveTime >= 2.5f)
            {
                showSavedAnimationLabel = false;

                return;
            }

            savedAnimationLabel.Draw();
        }
    }

    private Func<string, int, IDropdownItem> GetAnimationCategoryFormatter(AnimationSource source)
    {
        return source is AnimationSource.Custom ? CustomAnimationCategoryFormatter : GameAnimationCategoryFormatter;

        static LabelledDropdownItem CustomAnimationCategoryFormatter(string category, int index) =>
            new(category);

        LabelledDropdownItem GameAnimationCategoryFormatter(string category, int index) =>
            new(translation["poseGroupDropdown", category]);
    }

    private void OnAnimationAdded(object sender, AddedAnimationEventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (!animationCategoryComboBox.Contains(e.Animation.Category))
            animationCategoryComboBox.SetItems(customAnimationRepository.Categories);

        if (currentAnimationSource is not AnimationSource.Custom)
            return;

        var currentCategory = animationCategoryDropdown.SelectedItem;

        if (!animationCategoryDropdown.Contains(e.Animation.Category))
            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(AnimationSource.Custom));

        var currentCategoryIndex = animationCategoryDropdown.IndexOf(currentCategory, StringComparer.Ordinal);

        animationCategoryDropdown.SetSelectedIndexWithoutNotify(currentCategoryIndex);

        if (!string.Equals(e.Animation.Category, currentCategory, StringComparison.Ordinal))
            return;

        var currentAnimation = animationDropdown.SelectedItem;

        animationDropdown.SetItemsWithoutNotify(AnimationList(AnimationSource.Custom));

        var currentAnimationIndex = animationDropdown.IndexOf(currentAnimation);

        animationDropdown.SetSelectedIndexWithoutNotify(currentAnimationIndex);
    }

    private void OnCustomAnimationRepositoryRefreshed(object sender, EventArgs e)
    {
        var newCategories = AnimationCategoryList(AnimationSource.Custom).ToArray();

        animationCategoryComboBox.SetItems(newCategories);

        if (currentAnimationSource is not AnimationSource.Custom)
            return;

        if (CurrentAnimation?.Animation is not CustomAnimationModel animation)
            return;

        if (customAnimationRepository.ContainsCategory(animation.Category))
        {
            var currentCategory = animation.Category;
            var categoryIndex = newCategories.IndexOf(currentCategory, StringComparer.Ordinal);

            if (categoryIndex < 0)
            {
                animationCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
                animationDropdown.SetItemsWithoutNotify(AnimationList(AnimationSource.Custom), 0);

                return;
            }

            animationCategoryDropdown.SetItemsWithoutNotify(newCategories, categoryIndex);

            var newAnimations = AnimationList(AnimationSource.Custom).Cast<CustomAnimationModel>().ToArray();
            var animationIndex = newAnimations.FindIndex(newAnimation => newAnimation.ID == animation.ID);

            if (animationIndex < 0)
                animationIndex = 0;

            animationDropdown.SetItemsWithoutNotify(newAnimations, animationIndex);
        }
        else
        {
            animationCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
            animationDropdown.SetItemsWithoutNotify(AnimationList(AnimationSource.Custom), 0);
        }
    }

    private void ChangeAnimationSource(AnimationSource source)
    {
        currentAnimationSource = source;
        animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(currentAnimationSource), 0);
        animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(currentAnimationSource);

        animationDropdown.SetItemsWithoutNotify(AnimationList(currentAnimationSource), 0);

        if (animationDropdown.SelectedItem is not null)
            ChangeAnimation(animationDropdown.SelectedItem);

        var query = searchBar.Query;

        searchBar.ClearQuery();
        searchBar.SetQueryWithoutShowingResults(query);
    }

    private void OnAnimationCategoryChanged(object sender, DropdownEventArgs<string> e)
    {
        if (e.PreviousSelectedItemIndex == e.SelectedItemIndex)
            animationDropdown.SelectedItemIndex = 0;
        else
            animationDropdown.SetItems(AnimationList(currentAnimationSource), 0);
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Animation.PropertyChanged -= OnAnimationPropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Animation.PropertyChanged += OnAnimationPropertyChanged;

        UpdatePanel(CurrentAnimation.Animation);
    }

    private void OnRefreshButtonPushed(object sender, EventArgs e) =>
        customAnimationRepository.Refresh();

    private void OnSavePoseButtonPushed(object sender, EventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        var pose = characterSelectionController.Current.IK.GetAnimationData();
        var category = animationCategoryComboBox.Value;
        var name = animationNameTextField.Value;

        if (string.IsNullOrEmpty(category))
            category = customAnimationRepository.RootCategoryName;

        if (string.IsNullOrEmpty(name))
            name = "custom_pose";

        customAnimationRepository.Add(pose, category, name);

        animationNameTextField.Value = string.Empty;

        showSavedAnimationLabel = true;
        saveTime = Time.time;
    }

    private void UpdatePanel(IAnimationModel animation)
    {
        var animationSource = animation.Custom ? AnimationSource.Custom : AnimationSource.Game;

        if (animationSource is AnimationSource.Game && gameAnimationRepository.Busy)
            return;

        if (animationSource != currentAnimationSource)
        {
            currentAnimationSource = animationSource;
            animationSourceToggles[animationSource].SetEnabledWithoutNotify(true);

            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(animationSource));
            animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(animationSource);

            var categoryIndex = GetCategoryIndex(animation);

            animationCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            var newAnimationList = AnimationList(animationSource);
            var animationIndex = GetAnimationIndex(newAnimationList, animation);

            animationDropdown.SetItemsWithoutNotify(newAnimationList, animationIndex);
        }
        else
        {
            var categoryIndex = GetCategoryIndex(animation);
            var oldCategoryIndex = animationCategoryDropdown.SelectedItemIndex;

            animationCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                animationDropdown.SetItemsWithoutNotify(AnimationList(animationSource));

            var animationIndex = GetAnimationIndex(animationDropdown, animation);

            animationDropdown.SetSelectedIndexWithoutNotify(animationIndex);
        }

        int GetCategoryIndex(IAnimationModel currentAnimation)
        {
            var categoryIndex = animationCategoryDropdown.IndexOf(currentAnimation.Category, StringComparer.Ordinal);

            return categoryIndex < 0 ? 0 : categoryIndex;
        }

        int GetAnimationIndex(IEnumerable<IAnimationModel> animationList, IAnimationModel currentAnimation) =>
            currentAnimation.Custom
                ? customAnimationRepository.ContainsCategory(currentAnimation.Category)
                    ? animationList.IndexOf(currentAnimation)
                    : 0
                : gameAnimationRepository.ContainsCategory(currentAnimation.Category)
                    ? animationList.IndexOf(currentAnimation)
                    : 0;
    }

    private void OnAnimationChanged(object sender, DropdownEventArgs<IAnimationModel> e)
    {
        if (currentAnimationSource is AnimationSource.Game && gameAnimationRepository.Busy)
            return;

        if (e.Item is null)
            return;

        ChangeAnimation(e.Item);
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<IAnimationModel> e)
    {
        if (e.Item is null)
            return;

        ChangeAnimation(e.Item);
    }

    private void ChangeAnimation(IAnimationModel animation)
    {
        if (characterSelectionController.Current is not CharacterController character)
            return;

        if (Character.IK.Dirty)
        {
            characterUndoRedoService[Character].StartPoseChange();
            character.Animation.Apply(animation);
            characterUndoRedoService[Character].EndPoseChange();
        }
        else
        {
            character.Animation.Apply(animation);
        }
    }

    private void OnAnimationPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(AnimationController.Animation))
            return;

        var controller = (AnimationController)sender;

        if (controller.Animation.Equals(animationDropdown.SelectedItem))
            return;

        UpdatePanel(controller.Animation);
    }

    private IEnumerable<string> AnimationCategoryList(AnimationSource source) =>
        source switch
        {
            AnimationSource.Game => gameAnimationRepository.Busy ? [] : gameAnimationRepository.Categories,
            AnimationSource.Custom => customAnimationRepositorySorter.GetCategories(customAnimationRepository),
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };

    private IEnumerable<IAnimationModel> AnimationList(AnimationSource source)
    {
        return source is AnimationSource.Custom ? GetCustomAnimtions() : GetGameAnimations();

        IEnumerable<IAnimationModel> GetGameAnimations() =>
            gameAnimationRepository.Busy || !animationCategoryDropdown.Any() ? [] :
            gameAnimationRepository[animationCategoryDropdown.SelectedItem].Cast<IAnimationModel>();

        IEnumerable<IAnimationModel> GetCustomAnimtions() =>
            animationCategoryDropdown.Any() ? customAnimationRepositorySorter.GetAnimations(
                animationCategoryDropdown.SelectedItem, customAnimationRepository)
                .Cast<IAnimationModel>() :
            [];
    }
}
