namespace MeidoPhotoStudio.Database.Character;

public interface IBlendSetModel
{
    string Name { get; }

    string Category { get; }

    string BlendSetName { get; }

    bool Custom { get; }
}
