namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class BloomSchema
{
    public const short SchemaVersion = 1;

    public BloomSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public bool Active { get; init; }

    public float BloomValue { get; init; }

    public int BlurIterations { get; init; }

    public Color BloomThresholdColour { get; init; }

    public bool BloomHDR { get; init; }
}
