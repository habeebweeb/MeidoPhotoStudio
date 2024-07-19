using System.ComponentModel;

using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class AnimationSelectorPane : BasePane
{
    private const int GameAnimation = 0;
    private const int CustomAnimation = 1;

    private static readonly string[] AnimationSourceTranslationKeys = ["baseTab", "customTab"];

    private readonly SelectionGrid animationSourceGrid;
    private readonly Dropdown2<string> animationCategoryDropdown;
    private readonly Dropdown2<IAnimationModel> animationDropdown;
    private readonly GameAnimationRepository gameAnimationRepository;
    private readonly CustomAnimationRepository customAnimationRepository;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly PaneHeader paneHeader;
    private readonly ComboBox animationCategoryComboBox;
    private readonly TextField animationNameTextField;
    private readonly Toggle saveAnimationToggle;
    private readonly Button savePoseButton;
    private readonly Label initializingLabel;
    private readonly Label noAnimationsLabel;
    private readonly Header animationDirectoryHeader;
    private readonly Header animationFilenameHeader;
    private readonly Button refreshButton;

    public AnimationSelectorPane(
        GameAnimationRepository gameAnimationRepository,
        CustomAnimationRepository customAnimationRepository,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.gameAnimationRepository = gameAnimationRepository ?? throw new ArgumentNullException(nameof(gameAnimationRepository));
        this.customAnimationRepository = customAnimationRepository ?? throw new ArgumentNullException(nameof(customAnimationRepository));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.customAnimationRepository.AddedAnimation += OnAnimationAdded;
        this.customAnimationRepository.Refreshed += OnCustomAnimationRepositoryRefreshed;
        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        animationSourceGrid = new(Translation.GetArray("posePane", AnimationSourceTranslationKeys));
        animationSourceGrid.ControlEvent += OnAnimationSourceChanged;

        animationCategoryDropdown = new(
            GetAnimationCategoryFormatter(animationSourceGrid.SelectedItemIndex is CustomAnimation));

        animationCategoryDropdown.SelectionChanged += OnAnimationCategoryChanged;

        animationDropdown = new(
            AnimationList(animationSourceGrid.SelectedItemIndex is CustomAnimation),
            formatter: (model, index) => $"{index + 1}: {model.Name}");

        animationDropdown.SelectionChanged += OnAnimationChanged;

        paneHeader = new(Translation.Get("posePane", "header"), true);

        saveAnimationToggle = new(Translation.Get("posePane", "saveToggle"), false);
        animationCategoryComboBox = new([.. this.customAnimationRepository.Categories]);
        animationNameTextField = new();
        savePoseButton = new(Translation.Get("posePane", "saveButton"));
        savePoseButton.ControlEvent += OnSavePoseButtonPushed;

        refreshButton = new(Translation.Get("posePane", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        animationDirectoryHeader = new(Translation.Get("posePane", "categoryHeader"));
        animationFilenameHeader = new(Translation.Get("posePane", "nameHeader"));

        initializingLabel = new(Translation.Get("systemMessage", "initializing"));
        noAnimationsLabel = new(Translation.Get("posePane", "noAnimations"));

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
            if (animationSourceGrid.SelectedItemIndex is CustomAnimation)
                return;

            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(custom: false), 0);
            animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(custom: false);

            animationDropdown.SetItemsWithoutNotify(AnimationList(custom: false), 0);
        }
    }

    private AnimationController CurrentAnimation =>
        characterSelectionController.Current?.Animation;

    public override void Draw()
    {
        var enabled = characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        animationSourceGrid.Draw();
        MpsGui.BlackLine();

        GUI.enabled = enabled;

        if (animationSourceGrid.SelectedItemIndex is GameAnimation && gameAnimationRepository.Busy)
            initializingLabel.Draw();
        else
            DrawDropdowns();

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        saveAnimationToggle.Draw();

        if (animationSourceGrid.SelectedItemIndex is CustomAnimation)
        {
            GUILayout.FlexibleSpace();

            refreshButton.Draw();
        }

        GUILayout.EndHorizontal();

        if (saveAnimationToggle.Value)
            DrawAddAnimation();

        void DrawDropdowns()
        {
            if (!animationCategoryDropdown.Any())
            {
                noAnimationsLabel.Draw();
            }
            else if (!animationDropdown.Any())
            {
                DrawDropdown(animationCategoryDropdown);

                noAnimationsLabel.Draw();
            }
            else
            {
                DrawDropdown(animationCategoryDropdown);
                DrawDropdown(animationDropdown);
            }
        }

        void DrawAddAnimation()
        {
            var parentWidth = parent.WindowRect.width;
            var width = GUILayout.Width(parentWidth - 75f);

            animationDirectoryHeader.Draw();
            animationCategoryComboBox.Draw(width);

            animationFilenameHeader.Draw();
            GUILayout.BeginHorizontal(width);

            animationNameTextField.Draw();

            savePoseButton.Draw(GUILayout.ExpandWidth(false));

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
        animationSourceGrid.SetItemsWithoutNotify(Translation.GetArray("posePane", AnimationSourceTranslationKeys));

        if (animationSourceGrid.SelectedItemIndex is GameAnimation)
            animationCategoryDropdown.Reformat();

        paneHeader.Label = Translation.Get("posePane", "header");
        saveAnimationToggle.Label = Translation.Get("posePane", "saveToggle");
        savePoseButton.Label = Translation.Get("posePane", "saveButton");
        refreshButton.Label = Translation.Get("posePane", "refreshButton");
        animationDirectoryHeader.Text = Translation.Get("posePane", "categoryHeader");
        animationFilenameHeader.Text = Translation.Get("posePane", "nameHeader");
        initializingLabel.Text = Translation.Get("systemMessage", "initializing");
        noAnimationsLabel.Text = Translation.Get("posePane", "noAnimations");
    }

    private static Func<string, int, string> GetAnimationCategoryFormatter(bool custom)
    {
        return custom ? CustomAnimationCategoryFormatter : GameAnimationCategoryFormatter;

        static string CustomAnimationCategoryFormatter(string category, int index) =>
            category;

        static string GameAnimationCategoryFormatter(string category, int index) =>
            Translation.Get("poseGroupDropdown", category);
    }

    private void OnAnimationAdded(object sender, AddedAnimationEventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (animationSourceGrid.SelectedItemIndex is CustomAnimation)
        {
            if (!animationCategoryDropdown.Contains(e.Animation.Category))
            {
                animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(e.Animation.Custom));
                animationCategoryComboBox.BaseDropDown.SetDropdownItemsWithoutNotify([.. customAnimationRepository.Categories], 0);
            }

            animationDropdown.SetItemsWithoutNotify(AnimationList(e.Animation.Custom));
        }

        CurrentAnimation.Apply(e.Animation);
    }

    private void OnCustomAnimationRepositoryRefreshed(object sender, EventArgs e)
    {
        if (animationSourceGrid.SelectedItemIndex is not CustomAnimation)
            return;

        if (customAnimationRepository.ContainsCategory(animationCategoryDropdown.SelectedItem))
        {
            var currentCategory = animationCategoryDropdown.SelectedItem;
            var newCategories = AnimationCategoryList(custom: true).ToArray();

            animationCategoryComboBox.SetDropdownItems(newCategories);

            var categoryIndex = newCategories.IndexOf(category => string.Equals(currentCategory, category, StringComparison.Ordinal));

            if (categoryIndex < 0)
            {
                animationCategoryDropdown.SetItemsWithoutNotify(newCategories, 0);
                animationDropdown.SetItemsWithoutNotify(AnimationList(custom: true), 0);

                return;
            }

            animationCategoryDropdown.SetItemsWithoutNotify(newCategories, categoryIndex);

            var currentAnimationModel = animationDropdown.SelectedItem;

            var newAnimations = AnimationList(custom: true).ToArray();
            var animationIndex = newAnimations.IndexOf(animation => animation == currentAnimationModel);

            if (animationIndex < 0)
                animationIndex = 0;

            animationDropdown.SetItemsWithoutNotify(newAnimations, animationIndex);
        }
        else
        {
            var newCategories = AnimationCategoryList(custom: true).ToArray();

            animationCategoryDropdown.SetItems(newCategories, 0);
            animationCategoryComboBox.BaseDropDown.SetDropdownItemsWithoutNotify(newCategories);
        }
    }

    private void OnAnimationSourceChanged(object sender, EventArgs e)
    {
        var custom = animationSourceGrid.SelectedItemIndex is CustomAnimation;

        animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(custom), 0);
        animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(custom);

        animationDropdown.SetItemsWithoutNotify(AnimationList(custom), 0);

        if (animationDropdown.SelectedItem is not null)
            CurrentAnimation?.Apply(animationDropdown.SelectedItem);
    }

    private void OnAnimationCategoryChanged(object sender, DropdownEventArgs<string> e)
    {
        if (e.PreviousSelectedItemIndex == e.SelectedItemIndex)
            animationDropdown.SelectedItemIndex = 0;
        else
            animationDropdown.SetItems(AnimationList(animationSourceGrid.SelectedItemIndex is CustomAnimation), 0);
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
    }

    private void UpdatePanel(IAnimationModel animation)
    {
        var animationSource = animation.Custom ? CustomAnimation : GameAnimation;

        if (animationSource is GameAnimation && gameAnimationRepository.Busy)
            return;

        if (animationSource != animationSourceGrid.SelectedItemIndex)
        {
            animationSourceGrid.SetValueWithoutNotify(animationSource);

            var categoryIndex = GetCategoryIndex(animation);
            var custom = animationSourceGrid.SelectedItemIndex is CustomAnimation;

            animationCategoryDropdown.SetItemsWithoutNotify(AnimationCategoryList(animation.Custom), categoryIndex);
            animationCategoryDropdown.Formatter = GetAnimationCategoryFormatter(custom);

            var animationIndex = GetAnimationIndex(animation);

            animationDropdown.SetItemsWithoutNotify(AnimationList(animation.Custom), animationIndex);
        }
        else
        {
            var categoryIndex = GetCategoryIndex(animation);
            var oldCategoryIndex = animationCategoryDropdown.SelectedItemIndex;

            animationCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                animationDropdown.SetItemsWithoutNotify(AnimationList(animation.Custom));

            var animationIndex = GetAnimationIndex(animation);

            animationDropdown.SetSelectedIndexWithoutNotify(animationIndex);
        }

        int GetCategoryIndex(IAnimationModel currentAnimation)
        {
            var categoryIndex = animationCategoryDropdown.IndexOf(category =>
                string.Equals(category, currentAnimation.Category, StringComparison.OrdinalIgnoreCase));

            return categoryIndex < 0 ? 0 : categoryIndex;
        }

        int GetAnimationIndex(IAnimationModel currentAnimation)
        {
            if (currentAnimation.Custom)
            {
                var customAnimation = (CustomAnimationModel)currentAnimation;

                return customAnimationRepository.ContainsCategory(currentAnimation.Category)
                    ? animationDropdown
                        .Cast<CustomAnimationModel>()
                        .IndexOf(animation => animation.ID == customAnimation.ID)
                    : 0;
            }
            else
            {
                var gameAnimation = (GameAnimationModel)currentAnimation;

                return gameAnimationRepository.ContainsCategory(currentAnimation.Category)
                    ? animationDropdown
                        .Cast<GameAnimationModel>()
                        .IndexOf(animation => string.Equals(animation.ID, gameAnimation.ID, StringComparison.OrdinalIgnoreCase))
                    : 0;
            }
        }
    }

    private void OnAnimationChanged(object sender, DropdownEventArgs<IAnimationModel> e)
    {
        if (characterSelectionController.Current is null)
            return;

        if (animationSourceGrid.SelectedItemIndex is GameAnimation && gameAnimationRepository.Busy)
            return;

        if (e.Item is null)
            return;

        CurrentAnimation?.Apply(e.Item);
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

    private IEnumerable<string> AnimationCategoryList(bool custom) =>
        custom ? customAnimationRepository.Categories
            .OrderBy(category => !string.Equals(category, customAnimationRepository.RootCategoryName, StringComparison.Ordinal))
            .ThenBy(category => category, new WindowsLogicalStringComparer()) :
        gameAnimationRepository.Busy ? [] :
        gameAnimationRepository.Categories;

    private IEnumerable<IAnimationModel> AnimationList(bool custom)
    {
        return custom ? GetCustomAnimtions() : GetGameAnimations();

        IEnumerable<IAnimationModel> GetGameAnimations() =>
            gameAnimationRepository.Busy || !animationCategoryDropdown.Any() ? [] :
            gameAnimationRepository[animationCategoryDropdown.SelectedItem].Cast<IAnimationModel>();

        IEnumerable<IAnimationModel> GetCustomAnimtions() =>
            animationCategoryDropdown.Any() ? customAnimationRepository[animationCategoryDropdown.SelectedItem]
                .OrderBy(model => model.Name, new WindowsLogicalStringComparer())
                .Cast<IAnimationModel>() :
            [];
    }
}
