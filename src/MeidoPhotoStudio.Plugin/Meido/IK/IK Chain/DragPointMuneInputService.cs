using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointMuneInputService
    : DragPointInputRepository<DragPointMune>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointMuneInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointMune)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointMune)dragHandle);

    protected override DragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.RotateEyesChest].IsPressed())
            return DragHandleMode.RotateEyesChest;
        else if (inputConfiguration[Hotkey.RotateEyesChestAlternate].IsPressed())
            return DragHandleMode.RotateEyesChestAlternate;
        else
            return DragHandleMode.None;
    }
}
