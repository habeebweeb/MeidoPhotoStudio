using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class DragPointHeadInputService
    : DragPointInputRepository<DragPointHead>, IDragPointInputRepository<DragPointMeido>
{
    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointHead)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointHead)dragHandle);

    protected override DragType CheckDragType()
    {
        var shift = InputManager.Shift;
        var alt = InputManager.Alt;

        if (alt && InputManager.Control)
        {
            // eyes
            return shift
                ? DragType.MoveY
                : DragType.MoveXZ;
        }
        else if (alt)
        {
            // head
            return shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ;
        }
        else
        {
            return InputManager.GetKey(MpsKey.DragSelect)
                ? DragType.Select
                : DragType.None;
        }
    }
}
