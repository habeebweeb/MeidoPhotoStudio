namespace MeidoPhotoStudio.Plugin.Core.Database.Character;

public class CustomAnimationModel : IEquatable<CustomAnimationModel>, IAnimationModel
{
    public CustomAnimationModel(long id, string category, string animationFilename)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(animationFilename))
            throw new ArgumentException($"'{nameof(animationFilename)}' cannot be null or empty.", nameof(animationFilename));

        ID = id;
        Category = category;
        Filename = animationFilename;
    }

    public long ID { get; }

    public string Category { get; }

    public string Filename { get; }

    public string Name =>
        Path.GetFileNameWithoutExtension(Filename);

    public bool Custom =>
        true;

    public override bool Equals(object obj) =>
        Equals(obj as CustomAnimationModel);

    public bool Equals(IAnimationModel other) =>
        other is CustomAnimationModel model && Equals(model);

    public bool Equals(CustomAnimationModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return ID == other.ID
            && string.Equals(Category, other.Category, StringComparison.Ordinal)
            && string.Equals(Filename, other.Filename, StringComparison.Ordinal);
    }

    public override int GetHashCode() =>
        (ID, Category, Filename, Custom).GetHashCode();
}
