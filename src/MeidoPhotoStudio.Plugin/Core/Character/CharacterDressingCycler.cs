using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterDressingCycler : IInputHandler
{
    private static readonly TBody.MaskMode[] DressingModes =
        [TBody.MaskMode.None, TBody.MaskMode.Underwear, TBody.MaskMode.Nude];

    private readonly CharacterService characterService;
    private readonly InputConfiguration inputConfiguration;

    private int currentDressingMode = 0;

    public CharacterDressingCycler(CharacterService characterService, InputConfiguration inputConfiguration)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));

        this.characterService.CalledCharacters += OnCharactersCalled;
    }

    public bool Active =>
        characterService.Count > 0;

    public void CheckInput()
    {
        if (inputConfiguration[Shortcut.CycleMaidDressing].IsDown())
            CycleCharacterDressing();
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e) =>
        currentDressingMode = 0;

    private void CycleCharacterDressing()
    {
        if (characterService.Busy)
            return;

        if (characterService.Count is 0)
            return;

        currentDressingMode = ++currentDressingMode % DressingModes.Length;

        foreach (var character in characterService)
            character.Clothing.DressingMode = DressingModes[currentDressingMode];
    }
}
