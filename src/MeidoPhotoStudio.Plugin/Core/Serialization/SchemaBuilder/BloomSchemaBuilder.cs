using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BloomSchemaBuilder : ISchemaBuilder<BloomSchema, BloomEffectManager>
{
    public BloomSchema Build(BloomEffectManager bloom) =>
        new()
        {
            Active = bloom.Active,
            BloomValue = bloom.BloomValue,
            BlurIterations = bloom.BlurIterations,
            BloomThresholdColour = bloom.BloomThresholdColour,
            BloomHDR = bloom.BloomHDR,
        };
}
