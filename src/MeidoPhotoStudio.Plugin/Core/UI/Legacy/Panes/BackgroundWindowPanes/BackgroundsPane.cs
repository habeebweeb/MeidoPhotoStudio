using System.ComponentModel;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BackgroundsPane : BasePane
{
    private readonly BackgroundService backgroundService;
    private readonly BackgroundRepository backgroundRepository;
    private readonly BackgroundDragHandleService backgroundDragHandleService;
    private readonly Dropdown<BackgroundCategory> backgroundCategoryDropdown;
    private readonly Dropdown<BackgroundModel> backgroundDropdown;
    private readonly Toggle dragHandleEnabledToggle;
    private readonly Toggle backgroundVisibleToggle;
    private readonly Toggle colourModeToggle;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly PaneHeader paneHeader;

    public BackgroundsPane(
        BackgroundService backgroundService,
        BackgroundRepository backgroundRepository,
        BackgroundDragHandleService backgroundDragHandleService)
    {
        this.backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
        this.backgroundRepository = backgroundRepository ?? throw new ArgumentNullException(nameof(backgroundRepository));
        this.backgroundDragHandleService = backgroundDragHandleService ?? throw new ArgumentNullException(nameof(backgroundDragHandleService));

        this.backgroundService.PropertyChanged += OnBackgroundPropertyChanged;

        paneHeader = new(Translation.Get("backgroundsPane", "header"));

        backgroundCategoryDropdown = new(BackgroundCategoryFormatter);
        backgroundCategoryDropdown.SelectionChanged += OnChangedCategory;

        backgroundDropdown = new((model, _) => model.Name);
        backgroundDropdown.SelectionChanged += OnChangedBackground;

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

        redSlider.ControlEvent += OnColourSliderChanged;

        greenSlider = new(Translation.Get("backgroundsPane", "green"), 0f, 1f, backgroundColour.g, backgroundColour.g)
        {
            HasReset = true,
            HasTextField = true,
        };

        greenSlider.ControlEvent += OnColourSliderChanged;

        blueSlider = new(Translation.Get("backgroundsPane", "blue"), 0f, 1f, backgroundColour.b, backgroundColour.b)
        {
            HasReset = true,
            HasTextField = true,
        };

        blueSlider.ControlEvent += OnColourSliderChanged;

        static string BackgroundCategoryFormatter(BackgroundCategory category, int index)
        {
            var translationKey = category switch
            {
                BackgroundCategory.COM3D2 => "com3d2",
                BackgroundCategory.CM3D2 => "cm3d2",
                BackgroundCategory.MyRoomCustom => "myRoomCustom",
                _ => throw new InvalidEnumArgumentException(nameof(category), (int)category, typeof(BackgroundCategory)),
            };

            return Translation.Get("backgroundSource", translationKey);
        }
    }

    public override void Activate()
    {
        base.Activate();

        var background = backgroundService.CurrentBackground;

        InitializeCategoryDropdown(background);
        InitializeBackgroundDropdown(background);

        void InitializeCategoryDropdown(BackgroundModel background)
        {
            var categoryIndex = 0;
            var categories = backgroundRepository.Categories.ToArray();

            categoryIndex = categories.IndexOf(category => category == background.Category);

            if (categoryIndex < 0)
                categoryIndex = 0;

            backgroundCategoryDropdown.SetItemsWithoutNotify(categories, categoryIndex);
        }

        void InitializeBackgroundDropdown(BackgroundModel background)
        {
            var backgroundIndex = 0;
            var backgrounds = backgroundRepository[background.Category].ToArray();

            backgroundIndex = backgrounds.IndexOf(model => background == model);

            if (backgroundIndex < 0)
                backgroundIndex = 0;

            backgroundDropdown.SetItemsWithoutNotify(backgrounds, backgroundIndex);
        }
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        DrawDropdown(backgroundCategoryDropdown);
        DrawDropdown(backgroundDropdown);

        DrawToggles();

        MpsGui.BlackLine();

        DrawColourSliders();

        void DrawDropdown<T>(Dropdown<T> dropdown)
        {
            GUILayout.BeginHorizontal();

            const int ScrollBarWidth = 23;

            var buttonAndScrollbarSize = ScrollBarWidth + Utility.GetPix(20) * 2 + 5;
            var dropdownButtonWidth = parent.WindowRect.width - buttonAndScrollbarSize;

            dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

            var arrowLayoutOptions = GUILayout.ExpandWidth(false);

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.CyclePrevious();

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.CycleNext();

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

        paneHeader.Label = Translation.Get("backgroundsPane", "header");
        dragHandleEnabledToggle.Label = Translation.Get("backgroundsPane", "dragHandleVisible");
        backgroundVisibleToggle.Label = Translation.Get("backgroundsPane", "backgroundVisible");
        colourModeToggle.Label = Translation.Get("backgroundsPane", "colour");
        redSlider.Label = Translation.Get("backgroundsPane", "red");
        greenSlider.Label = Translation.Get("backgroundsPane", "green");
        blueSlider.Label = Translation.Get("backgroundsPane", "blue");
        backgroundCategoryDropdown.Reformat();
        backgroundDropdown.Reformat();
    }

    private void OnToggledDragHandleEnabled(object sender, EventArgs e) =>
        backgroundDragHandleService.Enabled = dragHandleEnabledToggle.Value;

    private void OnToggledBackgroundVisible(object sender, EventArgs e) =>
        backgroundService.BackgroundVisible = backgroundVisibleToggle.Value;

    private void OnColourSliderChanged(object sender, EventArgs e) =>
        backgroundService.BackgroundColour = new(redSlider.Value, greenSlider.Value, blueSlider.Value);

    private void OnChangedCategory(object sender, EventArgs e) =>
        backgroundDropdown.SetItems(backgroundRepository[backgroundCategoryDropdown.SelectedItem], 0);

    private void OnBackgroundPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var service = (BackgroundService)sender;

        if (e.PropertyName is nameof(BackgroundService.CurrentBackground))
        {
            UpdatePanel(service);
        }
        else if (e.PropertyName is nameof(BackgroundService.BackgroundVisible))
        {
            backgroundVisibleToggle.SetEnabledWithoutNotify(service.BackgroundVisible);
        }
        else if (e.PropertyName is nameof(BackgroundService.BackgroundColour))
        {
            redSlider.SetValueWithoutNotify(service.BackgroundColour.r);
            greenSlider.SetValueWithoutNotify(service.BackgroundColour.g);
            blueSlider.SetValueWithoutNotify(service.BackgroundColour.b);
        }

        void UpdatePanel(BackgroundService service)
        {
            if (service.CurrentBackground == backgroundDropdown.SelectedItem)
                return;

            var categoryIndex = GetCategoryIndex(service.CurrentBackground);
            var oldCategoryIndex = backgroundCategoryDropdown.SelectedItemIndex;

            backgroundCategoryDropdown.SetSelectedIndexWithoutNotify(categoryIndex);

            if (categoryIndex != oldCategoryIndex)
                backgroundDropdown.SetItemsWithoutNotify(backgroundRepository[service.CurrentBackground.Category]);

            var backgroundIndex = GetBackgroundIndex(service.CurrentBackground);

            backgroundDropdown.SetSelectedIndexWithoutNotify(backgroundIndex);

            int GetCategoryIndex(BackgroundModel background)
            {
                var categoryIndex = backgroundCategoryDropdown.IndexOf(category => category == background.Category);

                return categoryIndex < 0 ? 0 : categoryIndex;
            }

            int GetBackgroundIndex(BackgroundModel currentBackground)
            {
                var backgroundIndex = backgroundDropdown.IndexOf(background => currentBackground == background);

                return backgroundIndex < 0 ? 0 : backgroundIndex;
            }
        }
    }

    private void OnChangedBackground(object sender, EventArgs e) =>
        backgroundService.ChangeBackground(backgroundDropdown.SelectedItem);
}
