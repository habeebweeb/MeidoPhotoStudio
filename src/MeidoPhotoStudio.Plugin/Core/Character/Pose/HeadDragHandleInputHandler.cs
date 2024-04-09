using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class HeadDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<HeadDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private HeadMode currentMode;

    private enum HeadMode
    {
        None,
        Select,
        Rotate,
        RotateAlternate,
        RotateEyes,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((HeadDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((HeadDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = HeadMode.None;

        if (inputConfiguration[Hotkey.Select].IsPressed())
            newMode = HeadMode.Select;
        else if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            newMode = HeadMode.Rotate;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            newMode = HeadMode.RotateAlternate;
        else if (inputConfiguration[Hotkey.RotateEyesChest].IsPressed())
            newMode = HeadMode.RotateEyes;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(HeadDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(HeadMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(HeadDragHandleController controller, HeadMode mode) =>
        controller.CurrentMode = mode switch
        {
            HeadMode.None => controller.None,
            HeadMode.Select => controller.Select,
            HeadMode.Rotate => controller.Rotate,
            HeadMode.RotateAlternate => controller.RotateAlternate,
            HeadMode.RotateEyes => controller.RotateEyes,
            _ => controller.None,
        };
}
