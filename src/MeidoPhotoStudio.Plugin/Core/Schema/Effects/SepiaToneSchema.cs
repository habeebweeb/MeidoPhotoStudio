namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class SepiaToneSchema
{
    public const short SchemaVersion = 1;

    public SepiaToneSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public bool Active { get; init; }
}
