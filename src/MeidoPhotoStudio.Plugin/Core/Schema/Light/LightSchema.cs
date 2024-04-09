namespace MeidoPhotoStudio.Plugin.Core.Schema.Light;

public class LightSchema
{
    public const short SchemaVersion = 2;

    public LightSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public LightPropertiesSchema DirectionalProperties { get; init; }

    public LightPropertiesSchema SpotProperties { get; init; }

    public LightPropertiesSchema PointProperties { get; init; }

    public Vector3 Position { get; init; }

    public LightType Type { get; init; }

    public bool ColourMode { get; init; }

    public bool Enabled { get; init; }
}
