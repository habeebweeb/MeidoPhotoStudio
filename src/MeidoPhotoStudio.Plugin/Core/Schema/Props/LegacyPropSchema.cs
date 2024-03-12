namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class LegacyPropSchema
{
    public const short SchemaVersion = 2;

    public LegacyPropSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public PropInfoSchema PropInfo { get; init; }

    public TransformSchema Transform { get; init; }

    public AttachPointSchema AttachPoint { get; init; }

    public bool ShadowCasting { get; init; }

    public bool DragHandleEnabled { get; init; }

    public bool GizmoEnabled { get; init; }

    public CustomGizmo.GizmoMode GizmoMode { get; init; }

    public bool PropVisible { get; init; }
}
