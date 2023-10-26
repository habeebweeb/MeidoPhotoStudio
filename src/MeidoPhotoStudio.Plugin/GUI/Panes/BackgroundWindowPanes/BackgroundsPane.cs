using System;
using System.ComponentModel;
using System.Linq;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class BackgroundsPane : BasePane
{
    private readonly BackgroundService backgroundService;
    private readonly BackgroundRepository backgroundRepository;
    private readonly BackgroundDragHandleService backgroundDragHandleService;
    private readonly Dropdown backgroundCategoryDropdown;
    private readonly Dropdown backgroundDropdown;
    private readonly Toggle dragHandleEnabledToggle;
    private readonly Toggle backgroundVisibleToggle;
    private readonly Toggle colourModeToggle;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;

    private BackgroundCategory[] availableCategories;
    private bool updatingUI;
    private string backgroundHeader;

    public BackgroundsPane(
        BackgroundService backgroundService,
        BackgroundRepository backgroundRepository,
        BackgroundDragHandleService backgroundDragHandleService)
    {
        this.backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
        this.backgroundRepository = backgroundRepository ?? throw new ArgumentNullException(nameof(backgroundRepository));
        this.backgroundDragHandleService = backgroundDragHandleService ?? throw new ArgumentNullException(nameof(backgroundDragHandleService));

        backgroundHeader = Translation.Get("backgroundsPane", "header");

        backgroundCategoryDropdown = new(new[] { string.Empty });
        backgroundCategoryDropdown.SelectionChange += OnChangedCategory;

        backgroundDropdown = new(new[] { string.Empty });
        backgroundDropdown.SelectionChange += OnChangedBackground;

        dragHandleEnabledToggle = new(
            Translation.Get("backgroundsPane", "dragHandleVisible"), backgroundDragHandleService.Enabled);

        dragHandleEnabledToggle.ControlEvent += OnToggledDragHandleEnabled;

        backgroundVisibleToggle = new(
            Translation.Get("backgroundsPane", "backgroundVisible"), backgroundService.BackgroundVisible);

        backgroundVisibleToggle.ControlEvent += OnToggledBackgroundVisible;

        colourModeToggle = new(Translation.Get("backgroundsPane", "colour"));

        var backgroundColour = backgroundService.BackgroundColour;

        redSlider = new(Translation.Get("backgroundsPane", "red"), 0f, 1f, backgroundColour.r, backgroundColour.r)
        {
            HasReset = true,
            HasTextField = true,
        };

        redSlider.ControlEvent += OnChangedRedSlider;

        greenSlider = new(Translation.Get("backgroundsPane", "green"), 0f, 1f, backgroundColour.g, backgroundColour.g)
        {
            HasReset = true,
            HasTextField = true,
        };

        greenSlider.ControlEvent += OnChangedGreenSlider;

        blueSlider = new(Translation.Get("backgroundsPane", "blue"), 0f, 1f, backgroundColour.b, backgroundColour.b)
        {
            HasReset = true,
            HasTextField = true,
        };

        blueSlider.ControlEvent += OnChangedBlueSlider;
    }

    private Color SliderColours =>
        new(redSlider.Value, greenSlider.Value, blueSlider.Value);

    private BackgroundCategory CurrentCategory =>
        availableCategories[backgroundCategoryDropdown.SelectedItemIndex];

    public override void Activate()
    {
        base.Activate();

        availableCategories = backgroundRepository.Categories.ToArray();

        updatingUI = true;

        InitializeCategoryDropdown();
        InitializeBackgroundDropdown();

        updatingUI = false;

        void InitializeCategoryDropdown()
        {
            var categoryIndex = 0;

            if (backgroundService.CurrentBackground is var currentBackground)
                categoryIndex = Array.IndexOf(availableCategories, currentBackground.Category);

            if (categoryIndex < 0)
                categoryIndex = 0;

            backgroundCategoryDropdown.SetDropdownItems(GetBackgroundCategories(availableCategories), categoryIndex);
        }

        void InitializeBackgroundDropdown()
        {
            var backgroundCategory = BackgroundCategory.COM3D2;
            var backgroundIndex = 0;

            if (backgroundService.CurrentBackground is var currentBackground)
            {
                backgroundIndex = backgroundRepository[currentBackground.Category].IndexOf(currentBackground);

                if (backgroundIndex < 0)
                    backgroundIndex = 0;

                backgroundCategory = currentBackground.Category;
            }

            backgroundDropdown.SetDropdownItems(GetBackgrounds(backgroundCategory), backgroundIndex);
        }
    }

    public override void Draw()
    {
        MpsGui.Header(backgroundHeader);
        MpsGui.WhiteLine();

        DrawDropdown(backgroundCategoryDropdown);
        DrawDropdown(backgroundDropdown);

        DrawToggles();

        DrawColourSliders();

        static void DrawDropdown(Dropdown dropdown)
        {
            var arrowLayoutOptions = GUILayout.ExpandWidth(false);

            const float dropdownButtonWidth = 153f;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.Step(-1);

            dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.Step(1);

            GUILayout.EndHorizontal();
        }

        void DrawToggles()
        {
            GUILayout.BeginHorizontal();

            backgroundVisibleToggle.Draw();

            GUILayout.FlexibleSpace();

            dragHandleEnabledToggle.Draw();

            GUILayout.EndHorizontal();
        }

        void DrawColourSliders()
        {
            colourModeToggle.Draw();

            MpsGui.BlackLine();

            if (!colourModeToggle.Value)
                return;

            redSlider.Draw();
            greenSlider.Draw();
            blueSlider.Draw();
        }
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        backgroundHeader = Translation.Get("backgroundsPane", "header");
        dragHandleEnabledToggle.Label = Translation.Get("backgroundsPane", "dragHandleVisible");
        backgroundVisibleToggle.Label = Translation.Get("backgroundsPane", "backgroundVisible");
        colourModeToggle.Label = Translation.Get("backgroundsPane", "colour");
        redSlider.Label = Translation.Get("backgroundsPane", "red");
        greenSlider.Label = Translation.Get("backgroundsPane", "green");
        blueSlider.Label = Translation.Get("backgroundsPane", "blue");

        updatingUI = true;

        backgroundCategoryDropdown.SetDropdownItems(GetBackgroundCategories(availableCategories));
        backgroundDropdown.SetDropdownItems(GetBackgrounds(CurrentCategory));

        updatingUI = false;
    }

    private static string[] GetBackgroundCategories(BackgroundCategory[] categories)
    {
        return categories
            .Select(CategoryToTranslationKey)
            .Select(key => Translation.Get("backgroundSource", key))
            .ToArray();

        static string CategoryToTranslationKey(BackgroundCategory category) =>
            category switch
            {
                BackgroundCategory.COM3D2 => "com3d2",
                BackgroundCategory.CM3D2 => "cm3d2",
                BackgroundCategory.MyRoomCustom => "myRoomCustom",
                _ => throw new InvalidEnumArgumentException(nameof(category), (int)category, typeof(BackgroundCategory)),
            };
    }

    private string[] GetBackgrounds(BackgroundCategory category) =>
        backgroundRepository[category].Select(model => model.Name).ToArray();

    private void OnToggledDragHandleEnabled(object sender, EventArgs e) =>
        backgroundDragHandleService.Enabled = dragHandleEnabledToggle.Value;

    private void OnToggledBackgroundVisible(object sender, EventArgs e) =>
        backgroundService.BackgroundVisible = backgroundVisibleToggle.Value;

    private void OnChangedBlueSlider(object sender, EventArgs e) =>
        backgroundService.BackgroundColour = SliderColours;

    private void OnChangedGreenSlider(object sender, EventArgs e) =>
        backgroundService.BackgroundColour = SliderColours;

    private void OnChangedRedSlider(object sender, EventArgs e) =>
        backgroundService.BackgroundColour = SliderColours;

    private void OnChangedCategory(object sender, EventArgs e)
    {
        if (updatingUI)
            return;

        backgroundDropdown.SetDropdownItems(GetBackgrounds(CurrentCategory), 0);
    }

    private void OnChangedBackground(object sender, EventArgs e)
    {
        if (updatingUI)
            return;

        var model = backgroundRepository[CurrentCategory][backgroundDropdown.SelectedItemIndex];

        backgroundService.ChangeBackground(model);
    }
}
