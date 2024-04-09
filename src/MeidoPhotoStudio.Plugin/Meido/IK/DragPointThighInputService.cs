using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointThighInputService
    : DragPointInputRepository<DragPointThigh>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointThighInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointThigh)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointThigh)dragHandle);

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.DragUpperBone].IsPressed())
            return LegacyDragHandleMode.DragUpperBone;
        else
            return LegacyDragHandleMode.None;
    }
}
