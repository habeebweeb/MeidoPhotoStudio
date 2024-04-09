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

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.RotateEyesChest].IsPressed())
            return LegacyDragHandleMode.RotateEyesChest;
        else if (inputConfiguration[Hotkey.RotateEyesChestAlternate].IsPressed())
            return LegacyDragHandleMode.RotateEyesChestAlternate;
        else
            return LegacyDragHandleMode.None;
    }
}
