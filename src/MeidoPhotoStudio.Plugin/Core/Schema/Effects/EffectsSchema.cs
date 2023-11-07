namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class EffectsSchema
{
    public const short SchemaVersion = 1;

    public EffectsSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public BloomSchema Bloom { get; init; }

    public DepthOfFieldSchema DepthOfField { get; init; }

    public FogSchema Fog { get; init; }

    public VignetteSchema Vignette { get; init; }

    public SepiaToneSchema SepiaTone { get; init; }

    public BlurSchema Blur { get; init; }
}
