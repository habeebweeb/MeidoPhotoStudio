using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class DragPointMuneInputService
    : DragPointInputRepository<DragPointMune>, IDragPointInputRepository<DragPointMeido>
{
    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointMune)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointMune)dragHandle);

    protected override DragType CheckDragType() =>
        InputManager.Control && InputManager.Alt
            ? InputManager.Shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ
            : DragType.None;
}
