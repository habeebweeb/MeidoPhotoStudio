using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<ChestDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private ChestMode currentMode;

    private enum ChestMode
    {
        None,
        Drag,
        RotateGizmo,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((ChestDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((ChestDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = ChestMode.None;

        if (inputConfiguration[Hotkey.RotateEyesChest].IsPressed())
            newMode = ChestMode.Drag;
        else if (inputConfiguration[Hotkey.RotateEyesChestAlternate].IsPressed())
            newMode = ChestMode.RotateGizmo;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(ChestDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(ChestMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(ChestDragHandleController controller, ChestMode mode) =>
        controller.CurrentMode = mode switch
        {
            ChestMode.None => controller.None,
            ChestMode.Drag => controller.Drag,
            ChestMode.RotateGizmo => controller.RotateGizmo,
            _ => controller.None,
        };
}
