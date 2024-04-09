using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class HipDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<HipDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private HipMode currentMode;

    private enum HipMode
    {
        None,
        Rotate,
        MoveY,
        Ignore,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((HipDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((HipDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = HipMode.None;

        if (inputConfiguration[Hotkey.HipBoneRotation].IsPressed())
            newMode = HipMode.Rotate;
        else if (inputConfiguration[Hotkey.MoveLocalY].IsPressed())
            newMode = HipMode.MoveY;
        else if (OtherKeyPressed())
            newMode = HipMode.Ignore;

        UpdateDragHandleMode(newMode);

        bool OtherKeyPressed() =>
            inputConfiguration.KeyPool.Any(UnityEngine.Input.GetKey);
    }

    protected override void OnControllerAdded(HipDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(HipMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(HipDragHandleController controller, HipMode mode) =>
        controller.CurrentMode = mode switch
        {
            HipMode.None => controller.None,
            HipMode.Rotate => controller.Rotate,
            HipMode.MoveY => controller.MoveY,
            HipMode.Ignore => controller.Ignore,
            _ => controller.None,
        };
}
