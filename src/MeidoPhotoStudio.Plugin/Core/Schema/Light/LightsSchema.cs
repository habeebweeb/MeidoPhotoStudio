namespace MeidoPhotoStudio.Plugin.Core.Schema.Light;

public class LightsSchema(short version = LightsSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public List<LightSchema> Lights { get; init; }
}
