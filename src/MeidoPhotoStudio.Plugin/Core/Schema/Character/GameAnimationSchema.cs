namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class GameAnimationSchema(short version = GameAnimationSchema.SchemaVersion)
    : IAnimationModelSchema
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public string ID { get; init; }

    public bool Custom =>
        false;
}
