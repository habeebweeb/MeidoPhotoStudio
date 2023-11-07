using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BlurSchemaBuilder : ISchemaBuilder<BlurSchema, BlurEffectManager>
{
    public BlurSchema Build(BlurEffectManager blur) =>
        new()
        {
            Active = blur.Active,
            BlurSize = blur.BlurSize,
        };
}
