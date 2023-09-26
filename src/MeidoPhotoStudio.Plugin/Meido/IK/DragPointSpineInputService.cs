using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class DragPointSpineInputService
    : DragPointInputRepository<DragPointSpine>, IDragPointInputRepository<DragPointMeido>
{
    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointSpine)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointSpine)dragHandle);

    protected override DragType CheckDragType()
    {
        var shift = InputManager.Shift;
        var alt = InputManager.Alt;

        if (OtherDragType())
        {
            return DragType.Ignore;
        }
        else if (!InputManager.Control && alt && shift)
        {
            // gizmo thigh rotation
            return DragType.RotLocalXZ;
        }
        else if (alt)
        {
            return DragType.Ignore;
        }
        else if (shift)
        {
            return DragType.RotLocalY;
        }
        else if (InputManager.Control)
        {
            // hip y transform and spine gizmo rotation
            return DragType.MoveY;
        }
        else
        {
            return DragType.None;
        }
    }
}
