namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class DragHandleSchema
{
    public const short SchemaVersion = 1;

    public DragHandleSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; init; }

    public bool HandleEnabled { get; init; }

    public bool GizmoEnabled { get; init; }

    public CustomGizmo.GizmoMode GizmoSpace { get; init; }
}
