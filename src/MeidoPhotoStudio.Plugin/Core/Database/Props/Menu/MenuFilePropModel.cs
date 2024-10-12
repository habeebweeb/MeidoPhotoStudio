namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

/// <summary>Object representation of a .menu file.</summary>
public partial class MenuFilePropModel : IEquatable<MenuFilePropModel>, IPropModel
{
    private string name;

    public MenuFilePropModel(string filename, bool gameMenu)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

        Filename = filename;
        ID = Filename.ToLower();
        GameMenu = gameMenu;
    }

    public string ID { get; }

    public bool GameMenu { get; }

    public string Filename { get; }

    public string OriginalName { get; private init; }

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? OriginalName : value;
    }

    public MPN CategoryMpn { get; init; }

    public string IconFilename { get; init; }

    public float Priority { get; init; }

    public string ModelFilename { get; init; }

    public IEnumerable<MaterialChange> MaterialChanges { get; init; }

    public IEnumerable<ModelAnimation> ModelAnimations { get; init; }

    public IEnumerable<ModelMaterialAnimation> ModelMaterialAnimations { get; init; }

    public IEnumerable<MaterialTextureChange> MaterialTextureChanges { get; init; }

    public bool Equals(IPropModel other) =>
        other is MenuFilePropModel model && Equals(model);

    public bool Equals(MenuFilePropModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GameMenu == other.GameMenu
            && CategoryMpn == other.CategoryMpn
            && string.Equals(ID, other.ID, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() =>
        (ID, CategoryMpn, GameMenu).GetHashCode();
}
