using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropDragHandleSchemaBuilder : ISchemaBuilder<PropDragHandleSchema, PropDragHandleController>
{
    public PropDragHandleSchema Build(PropDragHandleController value) =>
        new()
        {
            HandleEnabled = value.CubeEnabled,
            GizmoEnabled = value.GizmoEnabled,
            GizmoSpace = value.GizmoMode,
        };
}
