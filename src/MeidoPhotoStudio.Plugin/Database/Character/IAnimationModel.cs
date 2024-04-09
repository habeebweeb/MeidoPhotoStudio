namespace MeidoPhotoStudio.Database.Character;

public interface IAnimationModel
{
    string Category { get; }

    string Name { get; }

    string Filename { get; }

    bool Custom { get; }
}
