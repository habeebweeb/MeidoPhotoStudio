using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using UnityEngine.UI;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class DigitBaseDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<DigitBaseDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private DigitBaseMode currentMode;

    private enum DigitBaseMode
    {
        None,
        DragAll,
        Drag1,
        Drag2,
        Drag3,
        Drag4,
        Drag5,
        Gizmo1,
        Gizmo2,
        Gizmo3,
        Gizmo4,
        Gizmo5,
        TwistAll,
        Twist1,
        Twist2,
        Twist3,
        Twist4,
        Twist5,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((DigitBaseDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((DigitBaseDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = DigitBaseMode.None;

        if (inputConfiguration[Hotkey.DragFinger].IsPressed())
            newMode = DigitBaseMode.DragAll;
        else if (inputConfiguration[Hotkey.DragFinger1].IsPressed())
            newMode = DigitBaseMode.Drag1;
        else if (inputConfiguration[Hotkey.DragFinger2].IsPressed())
            newMode = DigitBaseMode.Drag2;
        else if (inputConfiguration[Hotkey.DragFinger3].IsPressed())
            newMode = DigitBaseMode.Drag3;
        else if (inputConfiguration[Hotkey.DragFinger4].IsPressed())
            newMode = DigitBaseMode.Drag4;
        else if (inputConfiguration[Hotkey.DragFinger5].IsPressed())
            newMode = DigitBaseMode.Drag5;
        else if (inputConfiguration[Hotkey.GizmoFinger1].IsPressed())
            newMode = DigitBaseMode.Gizmo1;
        else if (inputConfiguration[Hotkey.GizmoFinger2].IsPressed())
            newMode = DigitBaseMode.Gizmo2;
        else if (inputConfiguration[Hotkey.GizmoFinger3].IsPressed())
            newMode = DigitBaseMode.Gizmo3;
        else if (inputConfiguration[Hotkey.GizmoFinger4].IsPressed())
            newMode = DigitBaseMode.Gizmo4;
        else if (inputConfiguration[Hotkey.GizmoFinger5].IsPressed())
            newMode = DigitBaseMode.Gizmo5;
        else if (inputConfiguration[Hotkey.TwistAllFingers].IsPressed())
            newMode = DigitBaseMode.TwistAll;
        else if (inputConfiguration[Hotkey.TwistFinger1].IsPressed())
            newMode = DigitBaseMode.Twist1;
        else if (inputConfiguration[Hotkey.TwistFinger2].IsPressed())
            newMode = DigitBaseMode.Twist2;
        else if (inputConfiguration[Hotkey.TwistFinger3].IsPressed())
            newMode = DigitBaseMode.Twist3;
        else if (inputConfiguration[Hotkey.TwistFinger4].IsPressed())
            newMode = DigitBaseMode.Twist4;
        else if (inputConfiguration[Hotkey.TwistFinger5].IsPressed())
            newMode = DigitBaseMode.Twist5;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(DigitBaseDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(DigitBaseMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(DigitBaseDragHandleController controller, DigitBaseMode mode) =>
        controller.CurrentMode = mode switch
        {
            DigitBaseMode.None => controller.None,
            DigitBaseMode.DragAll => controller.Drag,
            DigitBaseMode.Drag1 => controller.Drag1,
            DigitBaseMode.Drag2 => controller.Drag2,
            DigitBaseMode.Drag3 => controller.Drag3,
            DigitBaseMode.Drag4 => controller.Drag4,
            DigitBaseMode.Drag5 => controller.Drag5,
            DigitBaseMode.Gizmo1 => controller.Gizmo1,
            DigitBaseMode.Gizmo2 => controller.Gizmo2,
            DigitBaseMode.Gizmo3 => controller.Gizmo3,
            DigitBaseMode.Gizmo4 => controller.Gizmo4,
            DigitBaseMode.Gizmo5 => controller.Gizmo5,
            DigitBaseMode.TwistAll => controller.Twist,
            DigitBaseMode.Twist1 => controller.Twist1,
            DigitBaseMode.Twist2 => controller.Twist2,
            DigitBaseMode.Twist3 => controller.Twist3,
            DigitBaseMode.Twist4 => controller.Twist4,
            DigitBaseMode.Twist5 => controller.Twist5,
            _ => controller.None,
        };
}
