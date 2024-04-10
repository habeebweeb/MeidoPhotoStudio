namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class BloomSchema(short version = BloomSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Active { get; init; }

    public float BloomValue { get; init; }

    public int BlurIterations { get; init; }

    public Color BloomThresholdColour { get; init; }

    public bool BloomHDR { get; init; }
}
