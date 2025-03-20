using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterConfigurationController
{
    private readonly CharacterService characterService;
    private readonly IKDragHandleService ikDragHandleService;
    private readonly CharacterConfiguration characterConfiguration;
    private readonly List<CharacterController> currentCharacters = [];

    public CharacterConfigurationController(
        CharacterService characterService,
        IKDragHandleService ikDragHandleService,
        CharacterConfiguration characterConfiguration)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));
        this.characterConfiguration = characterConfiguration ?? throw new ArgumentNullException(nameof(characterConfiguration));

        this.characterService.CallingCharacters += OnCharactersCalling;
        this.characterService.CalledCharacters += OnCharactersCalled;
    }

    private void OnCharactersCalling(object sender, CharacterServiceEventArgs e)
    {
        currentCharacters.Clear();
        currentCharacters.AddRange(characterService);
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        foreach (var character in e.LoadedCharacters.Except(currentCharacters))
        {
            ikDragHandleService[character].BoneMode = characterConfiguration.PrecisePosingEnabled.Value;
            character.IK.LimitLimbRotations = characterConfiguration.LimitJointsEnabled.Value;
            character.IK.LimitDigitRotations = characterConfiguration.LimitDigitsEnabled.Value;
            character.Head.FreeLook = characterConfiguration.FreeLookEnabled.Value;
            character.Face.Blink = characterConfiguration.BlinkEnabled.Value;
        }
    }
}
