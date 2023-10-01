using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointHeadInputService
    : DragPointInputRepository<DragPointHead>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointHeadInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointHead)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointHead)dragHandle);

    protected override DragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.RotateEyesChest].IsPressed())
            return DragHandleMode.RotateEyesChest;
        else if (inputConfiguration[Hotkey.RotateEyesChestAlternate].IsPressed())
            return DragHandleMode.RotateEyesChestAlternate;
        else if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            return DragHandleMode.RotateBody;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            return DragHandleMode.RotateBodyAlternate;
        else if (inputConfiguration[Hotkey.Select].IsPressed())
            return DragHandleMode.Select;
        else
            return DragHandleMode.None;
    }
}
