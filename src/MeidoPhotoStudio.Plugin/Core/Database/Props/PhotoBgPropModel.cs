namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class PhotoBgPropModel(PhotoBGObjectData data, string name = "") : IEquatable<PhotoBgPropModel>, IPropModel
{
    private readonly PhotoBGObjectData data = data ?? throw new ArgumentNullException(nameof(data));

    private string name = string.IsNullOrEmpty(name) ? data.name : name;

    public string Name
    {
        get => name;
        set => name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string IconFilename =>
        string.Empty;

    public long ID =>
        data.id;

    public string Category =>
        data.category;

    public string PrefabName =>
        data.create_prefab_name;

    public string AssetName =>
        data.create_asset_bundle_name;

    public string DirectFilename =>
        data.direct_file;

    public bool Equals(IPropModel other) =>
        other is PhotoBgPropModel model && Equals(model);

    public bool Equals(PhotoBgPropModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return ID == other.ID && string.Equals(Category, other.Category, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() =>
        (ID, Category).GetHashCode();
}
