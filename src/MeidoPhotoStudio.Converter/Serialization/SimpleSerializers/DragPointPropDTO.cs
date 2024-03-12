using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Converter.Serialization;

public class DragPointPropDTO
{
    public TransformDTO TransformDTO { get; set; }

    public AttachPointInfo AttachPointInfo { get; set; }

    public PropInfo PropInfo { get; set; }

    public bool ShadowCasting { get; set; }

    public bool DragHandleEnabled { get; set; } = true;

    public bool GizmoEnabled { get; set; } = true;

    public CustomGizmo.GizmoMode GizmoMode { get; set; } = CustomGizmo.GizmoMode.World;

    public bool Visible { get; set; } = true;

    public void Deconstruct(out TransformDTO transform, out AttachPointInfo attachPointInfo, out bool shadowCasting)
    {
        transform = TransformDTO;
        attachPointInfo = AttachPointInfo;
        shadowCasting = ShadowCasting;
    }
}
