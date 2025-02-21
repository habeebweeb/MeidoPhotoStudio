using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
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
    }

    public override void Draw()
    {
        smallDragHandleToggle.Draw();
        characterTransformDragHandleToggle.Draw();
        autoSelectToggle.Draw();
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

    private void OnSettingsChanged(object sender, EventArgs e)
    {
        smallDragHandleToggle.SetEnabledWithoutNotify(configuration.SmallTransformCube.Value);
        characterTransformDragHandleToggle.SetEnabledWithoutNotify(configuration.CharacterTransformCube.Value);
        autoSelectToggle.SetEnabledWithoutNotify(configuration.AutomaticSelection.Value);
    }
}
