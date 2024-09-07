using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CallController : IEnumerable<CharacterModel>
{
    private readonly CharacterRepository characterRepository;
    private readonly CharacterService characterService;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly EditModeMaidService editModeMaidService;
    private readonly List<CharacterModel> selectedCharacters = [];
    private readonly HashSet<CharacterModel> selectedCharactersSet = [];

    private bool activeOnly;

    public CallController(
        CharacterRepository characterRepository,
        CharacterService characterService,
        CustomMaidSceneService customMaidSceneService,
        EditModeMaidService editModeMaidService)
    {
        this.characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        this.editModeMaidService = editModeMaidService ?? throw new ArgumentNullException(nameof(editModeMaidService));

        this.characterService.CallingCharacters += OnCharactersCalling;
    }

    public bool HasActiveCharacters =>
        characterService.Count > 0;

    public bool ActiveOnly
    {
        get => activeOnly;
        set
        {
            if (characterService.Count is 0)
                return;

            activeOnly = value;
        }
    }

    public int Count =>
        ActiveOnly ? characterService.Count : characterRepository.Count;

    public CharacterModel this[int index] =>
        (uint)index >= Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : ActiveOnly
                ? characterService.GetCharacterModel(index)
                : characterRepository[index];

    public IEnumerator<CharacterModel> GetEnumerator() =>
        ActiveOnly
            ? characterService.ActiveCharacterModels.GetEnumerator()
            : characterRepository.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public bool CharacterSelected(CharacterModel character) =>
        character is null
            ? throw new ArgumentNullException(nameof(character))
            : selectedCharactersSet.Contains(character);

    public int IndexOfSelectedCharacter(CharacterModel character) =>
        selectedCharacters.IndexOf(character);

    public void Select(CharacterModel character)
    {
        _ = character ?? throw new ArgumentNullException(nameof(character));

        if (selectedCharactersSet.Contains(character))
        {
            if (customMaidSceneService.EditScene && character == editModeMaidService.OriginalEditingCharacter)
                return;

            selectedCharacters.Remove(character);
            selectedCharactersSet.Remove(character);
        }
        else
        {
            selectedCharacters.Add(character);
            selectedCharactersSet.Add(character);
        }
    }

    public void ClearSelected()
    {
        selectedCharacters.Clear();
        selectedCharactersSet.Clear();
    }

    public void Call()
    {
        if (!selectedCharacters.Contains(editModeMaidService.EditingCharacter))
            editModeMaidService.SetEditingCharacter(editModeMaidService.OriginalEditingCharacter);

        characterService.Call(selectedCharacters);
    }

    internal void Activate()
    {
        ClearSelected();

        if (customMaidSceneService.EditScene)
            Select(editModeMaidService.OriginalEditingCharacter);

        activeOnly = false;
    }

    private void OnCharactersCalling(object sender, CharacterServiceEventArgs e)
    {
        if (e.LoadedCharacters.Length is 0)
            activeOnly = false;
    }
}
