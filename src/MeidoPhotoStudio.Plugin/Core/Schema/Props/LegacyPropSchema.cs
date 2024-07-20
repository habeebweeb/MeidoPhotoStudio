using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class LegacyPropSchema(short version = LegacyPropSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public PropInfoSchema PropInfo { get; init; }

    public TransformSchema Transform { get; init; }

    public AttachPointSchema AttachPoint { get; init; }

    public bool ShadowCasting { get; init; }

    public bool DragHandleEnabled { get; init; }

    public bool GizmoEnabled { get; init; }

    public CustomGizmo.GizmoMode GizmoMode { get; init; }

    public bool PropVisible { get; init; }
}
