using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class DragPointFingerInputService
    : DragPointInputRepository<DragPointFinger>, IDragPointInputRepository<DragPointMeido>
{
    static DragPointFingerInputService() =>
        InputManager.Register(MpsKey.DragFinger, UnityEngine.KeyCode.Space, "Show finger handles");

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointFinger)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointFinger)dragHandle);

    protected override DragType CheckDragType() =>
        !InputManager.GetKey(MpsKey.DragFinger)
            ? DragType.None
            : InputManager.Shift
                ? DragType.RotLocalY
                : DragType.MoveXZ;
}
