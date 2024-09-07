namespace MeidoPhotoStudio.Plugin.Core.Database.Background;

public class BackgroundModel : IEquatable<BackgroundModel>
{
    private string name;

    public BackgroundModel(BackgroundCategory category, string assetName, string name = "")
    {
        if (string.IsNullOrEmpty(assetName))
            throw new ArgumentException($"'{nameof(assetName)}' cannot be null or empty.", nameof(assetName));

        Category = category;
        AssetName = assetName;
        Name = string.IsNullOrEmpty(name) ? AssetName : name;
    }

    public string ID =>
        AssetName;

    public BackgroundCategory Category { get; }

    public string AssetName { get; }

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? AssetName : value;
    }

    public static bool operator ==(BackgroundModel lhs, BackgroundModel rhs) =>
        lhs is null ? rhs is null : lhs.Equals(rhs);

    public static bool operator !=(BackgroundModel lhs, BackgroundModel rhs) =>
        !(lhs == rhs);

    public override bool Equals(object other) =>
        Equals(other as BackgroundModel);

    public bool Equals(BackgroundModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Category == other.Category && string.Equals(AssetName, other.AssetName)
            && string.Equals(Name, other.Name);
    }

    public override int GetHashCode()
    {
        var hashCode = 1174575641;

        hashCode = hashCode * -1521134295 + Category.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AssetName);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);

        return hashCode;
    }
}
