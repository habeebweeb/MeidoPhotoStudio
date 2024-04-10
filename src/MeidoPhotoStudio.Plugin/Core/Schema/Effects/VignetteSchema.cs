namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class VignetteSchema(short version = VignetteSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Active { get; init; }

    public float Intensity { get; init; }

    public float Blur { get; init; }

    public float BlurSpread { get; init; }

    public float ChromaticAberration { get; init; }
}
