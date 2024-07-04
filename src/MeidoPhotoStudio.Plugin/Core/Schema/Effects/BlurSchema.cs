namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class BlurSchema(short version = BlurSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public bool Active { get; init; }

    public float BlurSize { get; init; }

    public int BlurIterations { get; init; }

    public int Downsample { get; init; }
}
