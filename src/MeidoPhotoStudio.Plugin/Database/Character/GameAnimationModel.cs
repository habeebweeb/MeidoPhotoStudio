using System;
using System.IO;

namespace MeidoPhotoStudio.Database.Character;

public class GameAnimationModel : IEquatable<GameAnimationModel>, IAnimationModel
{
    public GameAnimationModel(string category, string animationFilename)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(animationFilename))
            throw new ArgumentException($"'{nameof(animationFilename)}' cannot be null or empty.", nameof(animationFilename));

        Category = category;
        Filename = animationFilename;
    }

    public string ID =>
        Filename;

    public string Category { get; }

    public string Filename { get; }

    public string Name =>
        Path.GetFileNameWithoutExtension(Filename);

    public bool Custom =>
        false;

    public override bool Equals(object obj) =>
        Equals(obj as GameAnimationModel);

    public bool Equals(GameAnimationModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return string.Equals(ID, other.ID, StringComparison.Ordinal)
            && string.Equals(Category, other.Category, StringComparison.Ordinal)
            && string.Equals(Filename, other.Filename, StringComparison.Ordinal);
    }

    public override int GetHashCode() =>
        (ID, Category, Filename, Custom).GetHashCode();

    public override string ToString() =>
        base.ToString();
}
