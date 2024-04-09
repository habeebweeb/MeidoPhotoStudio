using System.Collections.ObjectModel;

namespace MeidoPhotoStudio.Database.Character;

public class CharacterRepository : IEnumerable<CharacterModel>
{
    private ReadOnlyCollection<CharacterModel> characters;

    public int Count =>
        Characters.Count;

    private ReadOnlyCollection<CharacterModel> Characters
    {
        get
        {
            characters ??= Initialize();

            return characters;
        }
    }

    public CharacterModel this[int index] =>
        (uint)index >= Characters.Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : Characters[index];

    public CharacterModel GetByID(string id) =>
        this.FirstOrDefault(character => string.Equals(id, character.ID, StringComparison.OrdinalIgnoreCase));

    public IEnumerator<CharacterModel> GetEnumerator() =>
        Characters.GetEnumerator();

    public void Refresh() =>
        characters = Initialize();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private static ReadOnlyCollection<CharacterModel> Initialize() =>
        GameMain.Instance.CharacterMgr.GetStockMaidList()
            .Select(maid => new CharacterModel(maid))
            .ToList()
            .AsReadOnly();
}
