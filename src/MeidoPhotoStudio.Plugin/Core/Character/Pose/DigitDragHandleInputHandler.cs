using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class DigitDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<DigitDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private DigitMode currentMode;

    private enum DigitMode
    {
        None,
        DragAll,
        Drag1,
        Drag2,
        Drag3,
        Drag4,
        Drag5,
        Rotate1,
        Rotate2,
        Rotate3,
        Rotate4,
        Rotate5,
        TwistAll,
        Twist1,
        Twist2,
        Twist3,
        Twist4,
        Twist5,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((DigitDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((DigitDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = DigitMode.None;

        if (inputConfiguration[Hotkey.DragFinger].IsPressed())
            newMode = DigitMode.DragAll;
        else if (inputConfiguration[Hotkey.DragFinger1].IsPressed())
            newMode = DigitMode.Drag1;
        else if (inputConfiguration[Hotkey.DragFinger2].IsPressed())
            newMode = DigitMode.Drag2;
        else if (inputConfiguration[Hotkey.DragFinger3].IsPressed())
            newMode = DigitMode.Drag3;
        else if (inputConfiguration[Hotkey.DragFinger4].IsPressed())
            newMode = DigitMode.Drag4;
        else if (inputConfiguration[Hotkey.DragFinger5].IsPressed())
            newMode = DigitMode.Drag5;
        else if (inputConfiguration[Hotkey.GizmoFinger1].IsPressed())
            newMode = DigitMode.Rotate1;
        else if (inputConfiguration[Hotkey.GizmoFinger2].IsPressed())
            newMode = DigitMode.Rotate2;
        else if (inputConfiguration[Hotkey.GizmoFinger3].IsPressed())
            newMode = DigitMode.Rotate3;
        else if (inputConfiguration[Hotkey.GizmoFinger4].IsPressed())
            newMode = DigitMode.Rotate4;
        else if (inputConfiguration[Hotkey.GizmoFinger5].IsPressed())
            newMode = DigitMode.Rotate5;
        else if (inputConfiguration[Hotkey.TwistAllFingers].IsPressed())
            newMode = DigitMode.TwistAll;
        else if (inputConfiguration[Hotkey.TwistFinger1].IsPressed())
            newMode = DigitMode.Twist1;
        else if (inputConfiguration[Hotkey.TwistFinger2].IsPressed())
            newMode = DigitMode.Twist2;
        else if (inputConfiguration[Hotkey.TwistFinger3].IsPressed())
            newMode = DigitMode.Twist3;
        else if (inputConfiguration[Hotkey.TwistFinger4].IsPressed())
            newMode = DigitMode.Twist4;
        else if (inputConfiguration[Hotkey.TwistFinger5].IsPressed())
            newMode = DigitMode.Twist5;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(DigitDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(DigitMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(DigitDragHandleController controller, DigitMode mode) =>
        controller.CurrentMode = mode switch
        {
            DigitMode.None => controller.None,
            DigitMode.DragAll => controller.Drag,
            DigitMode.Drag1 => controller.Drag1,
            DigitMode.Drag2 => controller.Drag2,
            DigitMode.Drag3 => controller.Drag3,
            DigitMode.Drag4 => controller.Drag4,
            DigitMode.Drag5 => controller.Drag5,
            DigitMode.Rotate1 => controller.Rotate1,
            DigitMode.Rotate2 => controller.Rotate2,
            DigitMode.Rotate3 => controller.Rotate3,
            DigitMode.Rotate4 => controller.Rotate4,
            DigitMode.Rotate5 => controller.Rotate5,
            DigitMode.TwistAll => controller.Twist,
            DigitMode.Twist1 => controller.Twist1,
            DigitMode.Twist2 => controller.Twist2,
            DigitMode.Twist3 => controller.Twist3,
            DigitMode.Twist4 => controller.Twist4,
            DigitMode.Twist5 => controller.Twist5,
            _ => controller.None,
        };
}
