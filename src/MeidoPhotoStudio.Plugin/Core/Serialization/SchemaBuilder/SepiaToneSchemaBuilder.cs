using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class SepiaToneSchemaBuilder : ISchemaBuilder<SepiaToneSchema, SepiaToneController>
{
    public SepiaToneSchema Build(SepiaToneController sepiaTone) =>
        new()
        {
            Active = sepiaTone.Active,
        };
}
