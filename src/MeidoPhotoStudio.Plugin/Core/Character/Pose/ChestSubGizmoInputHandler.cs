using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestSubGizmoInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<ChestSubGizmoController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private SubChestMode currentMode;

    private enum SubChestMode
    {
        None,
        Rotate,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((ChestSubGizmoController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((ChestSubGizmoController)controller);

    public override void CheckInput()
    {
        var newMode = SubChestMode.None;

        if (inputConfiguration[Hotkey.RotateEyesChestAlternate].IsPressed())
            newMode = SubChestMode.Rotate;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(ChestSubGizmoController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(SubChestMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(ChestSubGizmoController controller, SubChestMode mode) =>
        controller.CurrentMode = mode switch
        {
            SubChestMode.None => controller.None,
            SubChestMode.Rotate => controller.Rotate,
            _ => controller.None,
        };
}
