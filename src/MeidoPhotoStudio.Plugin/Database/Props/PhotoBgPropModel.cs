namespace MeidoPhotoStudio.Database.Props;

public class PhotoBgPropModel : IPropModel
{
    private readonly PhotoBGObjectData data;

    private string name;

    public PhotoBgPropModel(PhotoBGObjectData data, string name = "")
    {
        this.data = data ?? throw new ArgumentNullException(nameof(data));
        this.name = string.IsNullOrEmpty(name) ? data.name : name;
    }

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
}
