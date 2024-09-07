namespace MeidoPhotoStudio.Plugin.Core.Database.Character;

public class CustomBlendSetModel : IBlendSetModel
{
    public CustomBlendSetModel(long id, string category, string filename)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

        ID = id;
        Category = category;
        BlendSetName = filename;
    }

    public long ID { get; }

    public string Category { get; }

    public string Name =>
        Path.GetFileNameWithoutExtension(BlendSetName);

    public string BlendSetName { get; }

    public bool Custom =>
        true;

    public static bool operator ==(CustomBlendSetModel lhs, CustomBlendSetModel rhs) =>
        lhs is null ? rhs is null : lhs.Equals(rhs);

    public static bool operator !=(CustomBlendSetModel lhs, CustomBlendSetModel rhs) =>
        !(lhs == rhs);

    public override bool Equals(object other) =>
        other is CustomBlendSetModel model && Equals(model);

    public bool Equals(IBlendSetModel other) =>
        other is CustomBlendSetModel model && Equals(model);

    public bool Equals(CustomBlendSetModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return ID == other.ID
            && string.Equals(Category, other.Category, StringComparison.Ordinal)
            && string.Equals(BlendSetName, other.BlendSetName, StringComparison.Ordinal);
    }

    public override int GetHashCode() =>
        (ID, Category, BlendSetName, Custom).GetHashCode();
}
