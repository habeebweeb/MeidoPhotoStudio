using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointFingerInputService
    : DragPointInputRepository<DragPointFinger>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointFingerInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointFinger)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointFinger)dragHandle);

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.RotateFinger].IsPressed())
            return LegacyDragHandleMode.RotateFinger;
        else if (inputConfiguration[Hotkey.DragFinger].IsPressed())
            return LegacyDragHandleMode.DragFinger;
        else
            return LegacyDragHandleMode.None;
    }
}
