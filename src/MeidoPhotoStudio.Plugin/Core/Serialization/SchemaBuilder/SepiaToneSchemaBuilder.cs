using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class SepiaToneSchemaBuilder : ISchemaBuilder<SepiaToneSchema, SepiaToneEffectManager>
{
    public SepiaToneSchema Build(SepiaToneEffectManager sepiaTone) =>
        new()
        {
            Active = sepiaTone.Active,
        };
}
