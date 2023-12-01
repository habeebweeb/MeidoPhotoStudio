namespace MeidoPhotoStudio.Database.Props;

public class OtherPropModel : IPropModel
{
    private string name;

    public OtherPropModel(string assetName, string name = "")
    {
        AssetName = assetName;
        this.name = string.IsNullOrEmpty(name) ? AssetName : name;
    }

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? AssetName : value;
    }

    public string IconFilename =>
        string.Empty;

    public string ID =>
        AssetName;

    public string AssetName { get; init; }
}
