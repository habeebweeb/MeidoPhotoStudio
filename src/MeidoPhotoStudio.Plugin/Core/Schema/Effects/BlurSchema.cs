namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class BlurSchema
{
    public const short SchemaVersion = 1;

    public BlurSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public bool Active { get; init; }

    public float BlurSize { get; init; }
}
