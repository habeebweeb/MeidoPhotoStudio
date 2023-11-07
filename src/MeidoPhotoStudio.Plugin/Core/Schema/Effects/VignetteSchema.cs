namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class VignetteSchema
{
    public const short SchemaVersion = 1;

    public VignetteSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public bool Active { get; init; }

    public float Intensity { get; init; }

    public float Blur { get; init; }

    public float BlurSpread { get; init; }

    public float ChromaticAberration { get; init; }
}
