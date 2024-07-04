using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class VignetteSchemaBuilder : ISchemaBuilder<VignetteSchema, VignetteController>
{
    public VignetteSchema Build(VignetteController vignette) =>
        new()
        {
            Active = vignette.Active,
            Intensity = vignette.Intensity,
            Blur = vignette.Blur,
            BlurSpread = vignette.BlurSpread,
            ChromaticAberration = vignette.ChromaticAberration,
        };
}
