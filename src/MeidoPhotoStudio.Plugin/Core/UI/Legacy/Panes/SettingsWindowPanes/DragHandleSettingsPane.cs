using BepInEx.Configuration;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.UI;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class DragHandleSettingsPane : BasePane
{
    private readonly DragHandleConfiguration configuration;
    private readonly IKDragHandleService ikDragHandleService;
    private readonly PropDragHandleService propDragHandleService;
    private readonly GravityDragHandleService gravityDragHandleService;
    private readonly LightDragHandleService lightDragHandleRepository;
    private readonly BackgroundDragHandleService backgroundDragHandleService;
    private readonly Toggle smallDragHandleToggle;
    private readonly Toggle characterTransformDragHandleToggle;
    private readonly Toggle autoSelectToggle;
    private readonly Toggle autoSelectTabToggle;
    private readonly Header dragHandleColourHeader;
    private readonly ColourConfigurationSet upperLimbColourConfiguration;
    private readonly ColourConfigurationSet middleLimbColourConfiguration;
    private readonly ColourConfigurationSet lowerLimbColourConfiguration;
    private readonly ColourConfigurationSet spineColourConfiguration;
    private readonly ColourConfigurationSet rootColourConfiguration;
    private readonly ColourConfigurationSet baseDigitJointColourConfiguration;
    private readonly ColourConfigurationSet middleDigitJointColourConfiguration;
    private readonly ColourConfigurationSet tipDigitJointColourConfiguration;
    private readonly ColourConfigurationSet clothingColourConfiguration;
    private readonly ColourConfigurationSet hairColourConfiguration;

    public DragHandleSettingsPane(
        Translation translation,
        DragHandleConfiguration configuration,
        IKDragHandleService ikDragHandleService,
        PropDragHandleService propDragHandleService,
        GravityDragHandleService gravityDragHandleService,
        LightDragHandleService lightDragHandleRepository,
        BackgroundDragHandleService backgroundDragHandleService)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.gravityDragHandleService = gravityDragHandleService ?? throw new ArgumentNullException(nameof(gravityDragHandleService));
        this.lightDragHandleRepository = lightDragHandleRepository ?? throw new ArgumentNullException(nameof(lightDragHandleRepository));
        this.backgroundDragHandleService = backgroundDragHandleService ?? throw new ArgumentNullException(nameof(backgroundDragHandleService));

        this.configuration.SmallTransformCube.SettingChanged += OnSettingsChanged;
        this.configuration.CharacterTransformCube.SettingChanged += OnSettingsChanged;
        this.configuration.AutomaticSelection.SettingChanged += OnSettingsChanged;

        smallDragHandleToggle = new(
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "smallDragHandleToggle"),
            this.configuration.SmallTransformCube.Value);

        smallDragHandleToggle.ControlEvent += OnSmallDragHandleToggleChanged;

        characterTransformDragHandleToggle = new(
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "characterCubeDragHandleToggle"),
            this.configuration.CharacterTransformCube.Value);

        characterTransformDragHandleToggle.ControlEvent += OnCharacterTransformDragHandleToggleChanged;

        autoSelectToggle = new(
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "autoSelectObjectToggle"),
            this.configuration.AutomaticSelection.Value);

        autoSelectToggle.ControlEvent += OnAutoSelectToggleChanged;

        autoSelectTabToggle = new(
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "autoSelectTabToggle"),
            this.configuration.AutomaticTabSelection.Value);

        autoSelectTabToggle.ControlEvent += OnAutoSelectTabToggleChanged;

        var resetButtonLabel = new LocalizableGUIContent(translation, "dragHandleSettingsPane", "resetDragHandleColourButton");

        dragHandleColourHeader = new(new LocalizableGUIContent(translation, "dragHandleSettingsPane", "dragHandleColoursHeader"));

        upperLimbColourConfiguration = new(
            configuration.UpperLimbDragHandleColour,
            newColour => this.ikDragHandleService.UpperBoneColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "upperLimbColourLabel"),
            resetButtonLabel);

        middleLimbColourConfiguration = new(
            this.configuration.MiddleLimbDragHandleColour,
            newColour => this.ikDragHandleService.MiddleBoneColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "middleLimbColourLabel"),
            resetButtonLabel);

        lowerLimbColourConfiguration = new(
            this.configuration.LowerLimbDragHandleColour,
            newColour => this.ikDragHandleService.LowerBoneColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "lowerLimbColourLabel"),
            resetButtonLabel);

        spineColourConfiguration = new(
            this.configuration.SpineDragHandleColour,
            newColour => this.ikDragHandleService.SpineColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "spineColourLabel"),
            resetButtonLabel);

        rootColourConfiguration = new(
            this.configuration.RootDragHandleColour,
            newColour => this.ikDragHandleService.RootColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "rootColourLabel"),
            resetButtonLabel);

        baseDigitJointColourConfiguration = new(
            this.configuration.BaseDigitJointColour,
            newColour => this.ikDragHandleService.BaseDigitJointColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "baseDigitJointColourLabel"),
            resetButtonLabel);

        middleDigitJointColourConfiguration = new(
            this.configuration.MiddleDigitJointColour,
            newColour => this.ikDragHandleService.MiddleDigitJointColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "middleDigitJointColourLabel"),
            resetButtonLabel);

        tipDigitJointColourConfiguration = new(
            this.configuration.TipDigitJointColour,
            newColour => this.ikDragHandleService.TipDigitJointColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "tipDigitJointColourLabel"),
            resetButtonLabel);

        clothingColourConfiguration = new(
            this.configuration.ClothingDragHandleColour,
            newColour => this.gravityDragHandleService.ClothingDragHandleColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "clothingGravityColourLabel"),
            resetButtonLabel);

        hairColourConfiguration = new(
            this.configuration.HairDragHandleColour,
            newColour => this.gravityDragHandleService.HairDragHandleColour = newColour,
            new LocalizableGUIContent(translation, "dragHandleSettingsPane", "hairGravityColourLabel"),
            resetButtonLabel);
    }

    public override void Draw()
    {
        smallDragHandleToggle.Draw();
        characterTransformDragHandleToggle.Draw();
        autoSelectToggle.Draw();
        autoSelectTabToggle.Draw();

        UIUtility.DrawBlackLine();

        dragHandleColourHeader.Draw();

        upperLimbColourConfiguration.Draw();
        middleLimbColourConfiguration.Draw();
        lowerLimbColourConfiguration.Draw();
        spineColourConfiguration.Draw();
        rootColourConfiguration.Draw();

        baseDigitJointColourConfiguration.Draw();
        middleDigitJointColourConfiguration.Draw();
        tipDigitJointColourConfiguration.Draw();

        clothingColourConfiguration.Draw();
        hairColourConfiguration.Draw();
    }

    private void OnSmallDragHandleToggleChanged(object sender, EventArgs e)
    {
        configuration.SmallTransformCube.Value = smallDragHandleToggle.Value;

        propDragHandleService.SmallHandle = configuration.SmallTransformCube.Value;
        ikDragHandleService.SmallHandle = configuration.SmallTransformCube.Value;
        gravityDragHandleService.SmallHandle = configuration.SmallTransformCube.Value;
        lightDragHandleRepository.SmallHandle = configuration.SmallTransformCube.Value;
        backgroundDragHandleService.SmallHandle = configuration.SmallTransformCube.Value;
    }

    private void OnCharacterTransformDragHandleToggleChanged(object sender, EventArgs e)
    {
        configuration.CharacterTransformCube.Value = characterTransformDragHandleToggle.Value;

        ikDragHandleService.CubeEnabled = configuration.CharacterTransformCube.Value;
    }

    private void OnAutoSelectToggleChanged(object sender, EventArgs e)
    {
        configuration.AutomaticSelection.Value = autoSelectToggle.Value;

        propDragHandleService.AutoSelect = configuration.AutomaticSelection.Value;
        ikDragHandleService.AutoSelect = configuration.AutomaticSelection.Value;
        gravityDragHandleService.AutoSelect = configuration.AutomaticSelection.Value;
        lightDragHandleRepository.AutoSelect = configuration.AutomaticSelection.Value;
    }

    private void OnAutoSelectTabToggleChanged(object sender, EventArgs e)
    {
        configuration.AutomaticTabSelection.Value = autoSelectTabToggle.Value;

        propDragHandleService.AutoSelectTab = configuration.AutomaticTabSelection.Value;
        ikDragHandleService.AutoSelectTab = configuration.AutomaticTabSelection.Value;
        gravityDragHandleService.AutoSelectTab = configuration.AutomaticTabSelection.Value;
        lightDragHandleRepository.AutoSelectTab = configuration.AutomaticTabSelection.Value;
    }

    private void OnSettingsChanged(object sender, EventArgs e)
    {
        smallDragHandleToggle.SetEnabledWithoutNotify(configuration.SmallTransformCube.Value);
        characterTransformDragHandleToggle.SetEnabledWithoutNotify(configuration.CharacterTransformCube.Value);
        autoSelectToggle.SetEnabledWithoutNotify(configuration.AutomaticSelection.Value);
    }

    private class ColourConfigurationSet
    {
        private static readonly GUILayoutOption[] ColourButtonLayoutOptions;

        private readonly ConfigEntry<Color> configuration;
        private readonly Action<Color> updater;
        private readonly Label label;
        private readonly ColourPickerButton colourButton;
        private readonly Button resetButton;

        static ColourConfigurationSet()
        {
            ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;

            ColourButtonLayoutOptions =
            [
                GUILayout.Width(UIUtility.Scaled(60)),
                GUILayout.Height(UIUtility.Scaled(20)),
            ];

            static void OnScreenSizeChanged(object sender, EventArgs e)
            {
                ColourButtonLayoutOptions[0] = GUILayout.Width(UIUtility.Scaled(60));
                ColourButtonLayoutOptions[1] = GUILayout.Height(UIUtility.Scaled(20));
            }
        }

        public ColourConfigurationSet(
            ConfigEntry<Color> configuration, Action<Color> updater, GUIContent label, GUIContent resetButtonLabel)
        {
            this.configuration = configuration;
            this.updater = updater;
            this.label = new(label);

            colourButton = new(configuration.Value);
            colourButton.PickedColour += OnColourPicked;

            resetButton = new(resetButtonLabel);
            resetButton.ControlEvent += OnResetButtonPushed;

            configuration.SettingChanged += OnSettingChanged;
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();

            label.Draw();
            colourButton.Draw(ColourButtonLayoutOptions);
            resetButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        private void OnColourPicked(object sender, ColourPickerButtonEventArgs e) =>
            configuration.Value = e.Colour;

        private void OnResetButtonPushed(object sender, EventArgs e) =>
            configuration.Value = (Color)configuration.DefaultValue;

        private void OnSettingChanged(object sender, EventArgs e)
        {
            colourButton.Colour = configuration.Value;
            updater(configuration.Value);
        }
    }
}
