using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BloomSchemaBuilder : ISchemaBuilder<BloomSchema, BloomController>
{
    public BloomSchema Build(BloomController bloom) =>
        new()
        {
            Active = bloom.Active,
            BloomValue = bloom.BloomValue,
            BlurIterations = bloom.BlurIterations,
            BloomThresholdColour = bloom.BloomThresholdColour,
            BloomHDR = bloom.HDR,
        };
}
