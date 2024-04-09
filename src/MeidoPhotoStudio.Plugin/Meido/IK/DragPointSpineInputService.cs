using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointSpineInputService
    : DragPointInputRepository<DragPointSpine>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointSpineInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointSpine)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointSpine)dragHandle);

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.SpineBoneRotation].IsPressed())
            return LegacyDragHandleMode.SpineBoneRotation;
        else if (inputConfiguration[Hotkey.SpineBoneGizmoRotation].IsPressed())
            return LegacyDragHandleMode.SpineBoneGizmoRotation;
        else if (IgnoredInput())
            return LegacyDragHandleMode.Ignore;
        else
            return LegacyDragHandleMode.None;
    }

    private bool IgnoredInput() =>
        inputConfiguration[Hotkey.MoveWorldXZ].IsPressed() || inputConfiguration[Hotkey.MoveWorldY].IsPressed() ||
        inputConfiguration[Hotkey.Select].IsPressed() || inputConfiguration[Hotkey.Delete].IsPressed() ||
        inputConfiguration[Hotkey.MoveWorldXZ].IsPressed() || inputConfiguration[Hotkey.MoveWorldY].IsPressed() ||
        inputConfiguration[Hotkey.RotateWorldY].IsPressed() || inputConfiguration[Hotkey.RotateLocalY].IsPressed() ||
        inputConfiguration[Hotkey.RotateLocalXZ].IsPressed() || inputConfiguration[Hotkey.Scale].IsPressed() ||
        inputConfiguration[Hotkey.DragUpperBone].IsPressed() || inputConfiguration[Hotkey.DragMiddleBone].IsPressed() ||
        inputConfiguration[Hotkey.DragLowerBone].IsPressed();
}
