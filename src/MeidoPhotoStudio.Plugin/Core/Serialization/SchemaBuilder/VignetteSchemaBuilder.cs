using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class VignetteSchemaBuilder : ISchemaBuilder<VignetteSchema, VignetteEffectManager>
{
    public VignetteSchema Build(VignetteEffectManager vignette) =>
        new()
        {
            Active = vignette.Active,
            Intensity = vignette.Intensity,
            Blur = vignette.Blur,
            BlurSpread = vignette.BlurSpread,
            ChromaticAberration = vignette.ChromaticAberration,
        };
}
