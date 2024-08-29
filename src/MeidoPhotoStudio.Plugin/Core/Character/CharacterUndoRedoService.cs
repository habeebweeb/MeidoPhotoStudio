using MeidoPhotoStudio.Plugin.Core.UndoRedo;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterUndoRedoService
{
    private readonly CharacterService characterService;
    private readonly UndoRedoService undoRedoService;
    private readonly Dictionary<CharacterController, CharacterUndoRedoController> controllers = [];

    public CharacterUndoRedoService(CharacterService characterService, UndoRedoService undoRedoService)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.undoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));

        this.characterService.CalledCharacters += OnCharactersCalled;
    }

    public CharacterUndoRedoController this[CharacterController characterController] =>
        characterController is null
            ? throw new ArgumentNullException(nameof(characterController))
            : controllers[characterController];

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        var oldCharacters = controllers.Keys.ToArray();

        foreach (var character in oldCharacters.Except(e.LoadedCharacters))
            controllers.Remove(character);

        foreach (var character in e.LoadedCharacters.Except(oldCharacters))
            controllers[character] = new(character, undoRedoService);
    }
}
