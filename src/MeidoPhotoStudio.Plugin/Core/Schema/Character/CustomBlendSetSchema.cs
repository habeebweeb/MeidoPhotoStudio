namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class CustomBlendSetSchema(short version = CustomBlendSetSchema.SchemaVersion)
    : IBlendSetModelSchema
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public long ID { get; init; }

    public bool Custom =>
        true;
}
