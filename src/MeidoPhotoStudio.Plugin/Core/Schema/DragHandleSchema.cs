using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class DragHandleSchema(short version = DragHandleSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; init; } = version;

    public bool HandleEnabled { get; init; }

    public bool GizmoEnabled { get; init; }

    public CustomGizmo.GizmoMode GizmoSpace { get; init; }
}
