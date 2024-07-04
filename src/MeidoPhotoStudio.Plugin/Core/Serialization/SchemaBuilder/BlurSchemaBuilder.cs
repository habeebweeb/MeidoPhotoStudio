using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BlurSchemaBuilder : ISchemaBuilder<BlurSchema, BlurController>
{
    public BlurSchema Build(BlurController blur) =>
        new()
        {
            Active = blur.Active,
            BlurSize = blur.BlurSize,
            BlurIterations = blur.BlurIterations,
            Downsample = blur.Downsample,
        };
}
