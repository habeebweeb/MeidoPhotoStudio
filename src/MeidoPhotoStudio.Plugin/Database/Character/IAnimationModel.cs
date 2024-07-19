namespace MeidoPhotoStudio.Database.Character;

public interface IAnimationModel : IEquatable<IAnimationModel>
{
    string Category { get; }

    string Name { get; }

    string Filename { get; }

    bool Custom { get; }
}
