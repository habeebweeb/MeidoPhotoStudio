using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

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
        RotateAll,
        Rotate1,
        Rotate2,
        Rotate3,
        Rotate4,
        Rotate5,
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
        else if (inputConfiguration[Hotkey.RotateFinger].IsPressed())
            newMode = DigitBaseMode.RotateAll;
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
        else if (inputConfiguration[Hotkey.RotateFinger1].IsPressed())
            newMode = DigitBaseMode.Rotate1;
        else if (inputConfiguration[Hotkey.RotateFinger2].IsPressed())
            newMode = DigitBaseMode.Rotate2;
        else if (inputConfiguration[Hotkey.RotateFinger3].IsPressed())
            newMode = DigitBaseMode.Rotate3;
        else if (inputConfiguration[Hotkey.RotateFinger4].IsPressed())
            newMode = DigitBaseMode.Rotate4;
        else if (inputConfiguration[Hotkey.RotateFinger5].IsPressed())
            newMode = DigitBaseMode.Rotate5;

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
            DigitBaseMode.RotateAll => controller.Rotate,
            DigitBaseMode.Rotate1 => controller.Rotate1,
            DigitBaseMode.Rotate2 => controller.Rotate2,
            DigitBaseMode.Rotate3 => controller.Rotate3,
            DigitBaseMode.Rotate4 => controller.Rotate4,
            DigitBaseMode.Rotate5 => controller.Rotate5,
            _ => controller.None,
        };
}
