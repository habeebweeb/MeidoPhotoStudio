namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class SepiaToneSchema(short version = SepiaToneSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Active { get; init; }
}
