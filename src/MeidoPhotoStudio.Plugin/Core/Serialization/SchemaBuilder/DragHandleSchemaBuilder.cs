using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class DragHandleSchemaBuilder : ISchemaBuilder<DragHandleSchema, DragHandleControllerBase>
{
    public DragHandleSchema Build(DragHandleControllerBase value) =>
        new()
        {
            HandleEnabled = value.Enabled,
            GizmoEnabled = value.GizmoEnabled,
            GizmoSpace = value.GizmoMode,
        };
}
