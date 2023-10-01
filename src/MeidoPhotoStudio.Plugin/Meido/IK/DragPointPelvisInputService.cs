using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPelvisInputService
    : DragPointInputRepository<DragPointPelvis>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointPelvisInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointPelvis)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointPelvis)dragHandle);

    protected override DragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            return DragHandleMode.RotateBody;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            return DragHandleMode.RotateBodyAlternate;
        else
            return DragHandleMode.None;
    }
}
