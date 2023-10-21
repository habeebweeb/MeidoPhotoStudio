using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class DragPointHipInputService
    : DragPointInputRepository<DragPointHip>, IDragPointInputRepository<DragPointMeido>
{
    public DragPointHipInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    void IDragPointInputRepository<DragPointMeido>.AddDragHandle(DragPointMeido dragHandle) =>
        AddDragHandle((DragPointHip)dragHandle);

    void IDragPointInputRepository<DragPointMeido>.RemoveDragHandle(DragPointMeido dragHandle) =>
        RemoveDragHandle((DragPointHip)dragHandle);

    protected override DragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.HipBoneRotation].IsPressed())
            return DragHandleMode.HipBoneRotation;
        else if (inputConfiguration[Hotkey.MoveLocalY].IsPressed())
            return DragHandleMode.MoveLocalY;
        else if (IgnoredInput())
            return DragHandleMode.Ignore;
        else
            return DragHandleMode.None;
    }

    private bool IgnoredInput() =>
        inputConfiguration[Hotkey.Select].IsPressed() || inputConfiguration[Hotkey.Delete].IsPressed() ||
        inputConfiguration[Hotkey.MoveWorldXZ].IsPressed() || inputConfiguration[Hotkey.MoveWorldY].IsPressed() ||
        inputConfiguration[Hotkey.RotateWorldY].IsPressed() || inputConfiguration[Hotkey.RotateLocalY].IsPressed() ||
        inputConfiguration[Hotkey.RotateLocalXZ].IsPressed() || inputConfiguration[Hotkey.Scale].IsPressed() ||
        inputConfiguration[Hotkey.DragUpperBone].IsPressed() || inputConfiguration[Hotkey.DragMiddleBone].IsPressed() ||
        inputConfiguration[Hotkey.DragLowerBone].IsPressed();
}