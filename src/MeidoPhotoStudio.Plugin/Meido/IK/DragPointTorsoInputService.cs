using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointTorsoInputService
    : DragPointInputRepository<DragPointTorso>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointTorsoInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointTorso)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointTorso)dragHandle);

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            return LegacyDragHandleMode.RotateBody;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            return LegacyDragHandleMode.RotateBodyAlternate;
        else
            return LegacyDragHandleMode.None;
    }
}
