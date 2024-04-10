namespace MeidoPhotoStudio.Plugin.Core.Schema.Light;

public class LightSchema(short version = LightSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public LightPropertiesSchema DirectionalProperties { get; init; }

    public LightPropertiesSchema SpotProperties { get; init; }

    public LightPropertiesSchema PointProperties { get; init; }

    public Vector3 Position { get; init; }

    public LightType Type { get; init; }

    public bool ColourMode { get; init; }

    public bool Enabled { get; init; }
}
