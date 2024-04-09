using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointLimbInputService
    : DragPointInputRepository<DragPointLimb>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointLimbInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointLimb)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointLimb)dragHandle);

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.DragLowerLimb].IsPressed())
            return LegacyDragHandleMode.DragLowerLimb;
        else if (inputConfiguration[Hotkey.DragMiddleBone].IsPressed())
            return LegacyDragHandleMode.DragMiddleBone;
        else if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            return LegacyDragHandleMode.RotateBody;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            return LegacyDragHandleMode.RotateBodyAlternate;
        else if (IgnoredInput())
            return LegacyDragHandleMode.Ignore;
        else
            return LegacyDragHandleMode.None;
    }

    private bool IgnoredInput() =>
        inputConfiguration[Hotkey.DragFinger].IsPressed() || inputConfiguration[Hotkey.RotateFinger].IsPressed() ||
        inputConfiguration[Hotkey.Select].IsPressed() || inputConfiguration[Hotkey.Delete].IsPressed() ||
        inputConfiguration[Hotkey.MoveWorldXZ].IsPressed() || inputConfiguration[Hotkey.MoveWorldY].IsPressed() ||
        inputConfiguration[Hotkey.RotateWorldY].IsPressed() || inputConfiguration[Hotkey.RotateLocalY].IsPressed() ||
        inputConfiguration[Hotkey.RotateLocalXZ].IsPressed() || inputConfiguration[Hotkey.Scale].IsPressed() ||
        inputConfiguration[Hotkey.HipBoneRotation].IsPressed();
}
