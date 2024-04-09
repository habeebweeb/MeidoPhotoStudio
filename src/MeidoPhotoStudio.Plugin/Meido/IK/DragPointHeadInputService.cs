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

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.RotateEyesChest].IsPressed())
            return LegacyDragHandleMode.RotateEyesChest;
        else if (inputConfiguration[Hotkey.RotateEyesChestAlternate].IsPressed())
            return LegacyDragHandleMode.RotateEyesChestAlternate;
        else if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            return LegacyDragHandleMode.RotateBody;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            return LegacyDragHandleMode.RotateBodyAlternate;
        else if (inputConfiguration[Hotkey.Select].IsPressed())
            return LegacyDragHandleMode.Select;
        else
            return LegacyDragHandleMode.None;
    }
}
