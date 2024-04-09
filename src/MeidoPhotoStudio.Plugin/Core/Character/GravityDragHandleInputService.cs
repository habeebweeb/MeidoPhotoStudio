using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class GravityDragHandleInputService(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<GravityDragHandleController>(inputConfiguration)
{
    private GravityDragHandleMode currentDragHandleMode;

    private enum GravityDragHandleMode
    {
        MoveWorldXZ,
        MoveWorldY,
        Ignore,
    }

    public override void CheckInput()
    {
        var newMode = GravityDragHandleMode.MoveWorldXZ;

        if (inputConfiguration[Hotkey.MoveGravityWorldY].IsPressed())
            newMode = GravityDragHandleMode.MoveWorldY;
        else if (OtherKeyPressed())
            newMode = GravityDragHandleMode.Ignore;

        UpdateDragHandleMode(newMode);

        bool OtherKeyPressed() =>
            inputConfiguration.KeyPool.Any(UnityEngine.Input.GetKey);
    }

    private void UpdateDragHandleMode(GravityDragHandleMode newMode)
    {
        if (newMode == currentDragHandleMode)
            return;

        currentDragHandleMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentDragHandleMode);
    }

    private void ApplyDragHandleMode(GravityDragHandleController controller, GravityDragHandleMode mode) =>
        controller.CurrentMode = mode switch
        {
            GravityDragHandleMode.MoveWorldXZ => controller.MoveWorldXZ,
            GravityDragHandleMode.MoveWorldY => controller.MoveWorldY,
            GravityDragHandleMode.Ignore => controller.Ignore,
            _ => controller.MoveWorldXZ,
        };
}
