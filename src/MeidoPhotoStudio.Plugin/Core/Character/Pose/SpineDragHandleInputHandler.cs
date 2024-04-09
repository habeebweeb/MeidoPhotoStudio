using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class SpineDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<SpineDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private SpineMode currentMode;

    private enum SpineMode
    {
        None,
        Rotate,
        RotateAlternate,
        Ignore,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((SpineDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((SpineDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = SpineMode.None;

        if (inputConfiguration[Hotkey.SpineBoneRotation].IsPressed())
            newMode = SpineMode.Rotate;
        else if (inputConfiguration[Hotkey.SpineBoneGizmoRotation].IsPressed())
            newMode = SpineMode.RotateAlternate;
        else if (OtherKeyPressed())
            newMode = SpineMode.Ignore;

        UpdateDragHandleMode(newMode);

        bool OtherKeyPressed() =>
            inputConfiguration.KeyPool.Any(UnityEngine.Input.GetKey);
    }

    protected override void OnControllerAdded(SpineDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(SpineMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(SpineDragHandleController controller, SpineMode mode) =>
        controller.CurrentMode = mode switch
        {
            SpineMode.None => controller.None,
            SpineMode.Rotate => controller.Rotate,
            SpineMode.RotateAlternate => controller.RotateAlternate,
            SpineMode.Ignore => controller.Ignore,
            _ => controller.None,
        };
}
