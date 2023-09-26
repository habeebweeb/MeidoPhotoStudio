using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPelvisInputService
    : DragPointInputRepository<DragPointPelvis>, IDragPointInputRepository<DragPointMeido>
{
    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointPelvis)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointPelvis)dragHandle);

    protected override DragType CheckDragType() =>
        InputManager.Alt && !InputManager.Control
            ? InputManager.Shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ
            : DragType.None;
}
