namespace MeidoPhotoStudio.Plugin.Core.Schema.Light;

public class LightRepositorySchema
{
    public const short SchemaVersion = 1;

    public LightRepositorySchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public List<LightSchema> Lights { get; init; }
}
