using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class DragPointLimbInputService
    : DragPointInputRepository<DragPointLimb>, IDragPointInputRepository<DragPointMeido>
{
    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointLimb)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointLimb)dragHandle);

    protected override DragType CheckDragType()
    {
        var control = InputManager.Control;
        var alt = InputManager.Alt;

        // Check for DragMove so that hand dragpoint is not in the way
        if (OtherDragType())
            return DragType.Ignore;
        else if (control && !InputManager.GetKey(MpsKey.DragMove))
            return alt ? DragType.RotY : DragType.MoveXZ;
        else if (alt)
            return InputManager.Shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ;
        else
            return InputManager.Shift
                ? DragType.Ignore
                : DragType.None;
    }
}
