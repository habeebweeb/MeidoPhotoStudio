using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class DragPointTorsoInputService
    : DragPointInputRepository<DragPointTorso>, IDragPointInputRepository<DragPointMeido>
{
    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointTorso)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointTorso)dragHandle);

    protected override DragType CheckDragType() =>
        InputManager.Alt && !InputManager.Control
            ? InputManager.Shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ
            : DragType.None;
}
