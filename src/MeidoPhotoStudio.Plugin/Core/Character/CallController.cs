using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CallController : IEnumerable<CharacterModel>, INotifyPropertyChanged
{
    private static readonly IComparer<CharacterModel> DefaultComparer =
        ComparisonComparer<CharacterModel>.Create(CompareDefault);

    private static readonly IComparer<CharacterModel> DefaultNoScheduleComparer =
        ComparisonComparer<CharacterModel>.Create(CompareDefaultNoSchedule);

    private static readonly IComparer<CharacterModel> FirstNameComparer =
        ComparisonComparer<CharacterModel>.Create(CompareFirstName);

    private static readonly IComparer<CharacterModel> LastNameComparer =
        ComparisonComparer<CharacterModel>.Create(CompareLastName);

    private readonly IComparer<CharacterModel> noSortingComparer;
    private readonly CharacterRepository characterRepository;
    private readonly CharacterService characterService;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly EditModeMaidService editModeMaidService;
    private readonly List<CharacterModel> selectedCharacters = [];
    private readonly HashSet<CharacterModel> selectedCharactersSet = [];
    private readonly List<CharacterModel> characters = [];
    private readonly Dictionary<CharacterModel, int> noSortingMap = [];

    private string searchQuery;
    private bool activeOnly;
    private SortType currentSortType;
    private bool descending;
    private IComparer<CharacterModel> sortComparer = DefaultComparer;

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

        this.characterService.CalledCharacters += OnCharactersCalled;

        noSortingComparer = ComparisonComparer<CharacterModel>.Create(CompareNoSorting);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public enum SortType
    {
        None,
        Default,
        DefaultNoSchedule,
        FirstName,
        LastName,
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

            UpdateCharacterList();

            RaisePropertyChanged(nameof(ActiveOnly));
        }
    }

    public SortType Sort
    {
        get => currentSortType;
        set
        {
            if (currentSortType == value)
                return;

            currentSortType = value;

            sortComparer = currentSortType switch
            {
                SortType.None => noSortingComparer,
                SortType.Default => DefaultComparer,
                SortType.DefaultNoSchedule => DefaultNoScheduleComparer,
                SortType.FirstName => FirstNameComparer,
                SortType.LastName => LastNameComparer,
                _ => DefaultComparer,
            };

            UpdateCharacterList();

            RaisePropertyChanged(nameof(Sort));
        }
    }

    public bool Descending
    {
        get => descending;
        set
        {
            if (descending == value)
                return;

            descending = value;

            UpdateCharacterList();

            RaisePropertyChanged(nameof(Descending));
        }
    }

    public int Count =>
        characters.Count;

    public CharacterModel this[int index] =>
        (uint)index >= Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : characters[index];

    public IEnumerator<CharacterModel> GetEnumerator() =>
        characters.GetEnumerator();

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

        if (!customMaidSceneService.EditScene)
            return;

        selectedCharacters.Add(editModeMaidService.OriginalEditingCharacter);
        selectedCharactersSet.Add(editModeMaidService.OriginalEditingCharacter);
    }

    public void Call()
    {
        if (customMaidSceneService.EditScene)
        {
            if (!selectedCharacters.Contains(editModeMaidService.EditingCharacter))
                editModeMaidService.SetEditingCharacter(editModeMaidService.OriginalEditingCharacter);

            if (!selectedCharacters.Contains(editModeMaidService.OriginalEditingCharacter))
            {
                Utility.LogDebug($"Original editing character was not in the set of characters to call");

                selectedCharacters.Insert(0, editModeMaidService.OriginalEditingCharacter);
                selectedCharactersSet.Add(editModeMaidService.OriginalEditingCharacter);
            }
        }

        characterService.Call(selectedCharacters);
    }

    public void Search(string query)
    {
        if (string.Equals(searchQuery, query, StringComparison.CurrentCultureIgnoreCase))
            return;

        if (string.IsNullOrEmpty(query))
            query = string.Empty;

        searchQuery = query;

        UpdateCharacterList();
    }

    internal void Activate()
    {
        ClearSelected();

        searchQuery = string.Empty;
        currentSortType = SortType.None;
        sortComparer = noSortingComparer;
        descending = false;
        activeOnly = false;

        noSortingMap.Clear();

        for (var i = 0; i < characterRepository.Count; i++)
            noSortingMap[characterRepository[i]] = i;

        characters.Clear();
        characters.AddRange(characterRepository.OrderBy(sortComparer));

        if (customMaidSceneService.EditScene)
            Select(editModeMaidService.OriginalEditingCharacter);

        RaisePropertyChanged(nameof(Sort));
        RaisePropertyChanged(nameof(Descending));
        RaisePropertyChanged(nameof(ActiveOnly));
    }

    private static int CompareDefault(CharacterModel a, CharacterModel b) =>
        CharacterSelectManager.SortMaidStandard(a.Maid, b.Maid);

    private static int CompareDefaultNoSchedule(CharacterModel a, CharacterModel b) =>
        CharacterSelectManager.SortMaidStandardNoSchedule(a.Maid, b.Maid);

    private static int CompareFirstName(CharacterModel a, CharacterModel b) =>
        string.Compare(a.FirstName, b.FirstName);

    private static int CompareLastName(CharacterModel a, CharacterModel b) =>
        string.Compare(a.LastName, b.LastName);

    private int CompareNoSorting(CharacterModel a, CharacterModel b) =>
        noSortingMap[a].CompareTo(noSortingMap[b]);

    private void UpdateCharacterList()
    {
        characters.Clear();

        var characterList = ActiveOnly
            ? characterService.ActiveCharacterModels
            : characterRepository;

        var filteredCharacters = characterList;

        if (!string.IsNullOrEmpty(searchQuery))
            filteredCharacters = filteredCharacters
                .Where(character => character.FullName().Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase));

        characters.AddRange(filteredCharacters.OrderBy(sortComparer, Descending));
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        if (e.LoadedCharacters.Length is 0)
            ActiveOnly = false;
        else
            UpdateCharacterList();
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
