using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class AutomaticCharacterPlacementController : IActivateable
{
    private readonly CharacterService characterService;
    private readonly PlacementService placementService;
    private readonly CustomMaidSceneService customMaidSceneService;

    private bool editModeStart;
    private bool firstRun;
    private PlacementService.Placement placementType = PlacementService.Placement.Parabolic;

    public AutomaticCharacterPlacementController(
        CustomMaidSceneService customMaidSceneService,
        CharacterService characterService,
        PlacementService placementService)
    {
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.placementService = placementService ?? throw new ArgumentNullException(nameof(placementService));

        this.characterService.CalledCharacters += OnCharactersCalled;
    }

    public bool Enabled { get; set; }

    public PlacementService.Placement PlacementType
    {
        get => placementType;
        set
        {
            if (placementType == value)
                return;

            if (!Enum.IsDefined(typeof(PlacementService.Placement), value))
                throw new ArgumentOutOfRangeException(nameof(value));

            placementType = value;
        }
    }

    void IActivateable.Activate()
    {
        editModeStart = customMaidSceneService.EditScene;
        firstRun = true;
    }

    void IActivateable.Deactivate()
    {
        editModeStart = false;
        firstRun = true;
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        if (editModeStart)
        {
            editModeStart = false;

            return;
        }

        if (!Enabled || !firstRun)
            return;

        firstRun = false;

        placementService.ApplyPlacement(PlacementType);
    }
}
