namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class DepthOfFieldSchema(short version = DepthOfFieldSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Active { get; init; }

    public float FocalLength { get; init; }

    public float FocalSize { get; init; }

    public float Aperture { get; init; }

    public float MaxBlurSize { get; init; }

    public bool VisualizeFocus { get; init; }
}
