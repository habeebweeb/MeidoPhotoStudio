using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropShapeKeySchemaBuilder : ISchemaBuilder<PropShapeKeySchema, ShapeKeyController>
{
    public PropShapeKeySchema Build(ShapeKeyController value) =>
        new()
        {
            BlendValues = value is null ? [] : value.ToDictionary(kvp => kvp.HashKey, kvp => kvp.BlendValue),
        };
}
