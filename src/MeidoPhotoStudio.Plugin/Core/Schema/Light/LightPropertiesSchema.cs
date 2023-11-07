using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Schema.Light;

public class LightPropertiesSchema
{
    public const short SchemaVersion = 1;

    public LightPropertiesSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public Quaternion Rotation { get; init; }

    public float Intensity { get; init; }

    public float Range { get; init; }

    public float SpotAngle { get; init; }

    public float ShadowStrength { get; init; }

    public Color Colour { get; init; }
}
