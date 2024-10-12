namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class OtherPropModel : IEquatable<OtherPropModel>, IPropModel
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

    public bool Equals(IPropModel other) =>
        other is OtherPropModel model && Equals(model);

    public bool Equals(OtherPropModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return string.Equals(ID, other.ID, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() =>
        ID.GetHashCode();
}
