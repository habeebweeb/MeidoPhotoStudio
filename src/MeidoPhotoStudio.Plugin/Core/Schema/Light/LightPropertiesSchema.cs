namespace MeidoPhotoStudio.Plugin.Core.Schema.Light;

public class LightPropertiesSchema(short version = LightPropertiesSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public Quaternion Rotation { get; init; }

    public float Intensity { get; init; }

    public float Range { get; init; }

    public float SpotAngle { get; init; }

    public float ShadowStrength { get; init; }

    public Color Colour { get; init; }
}
