namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class GameBlendSetSchema(short version = GameBlendSetSchema.SchemaVersion) : IBlendSetModelSchema
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public int ID { get; init; }

    public bool Custom =>
        false;
}
