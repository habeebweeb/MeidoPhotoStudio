using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ThighDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<ThighGizmoController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private ThighMode currentMode;

    private enum ThighMode
    {
        None,
        Rotate,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((ThighGizmoController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((ThighGizmoController)controller);

    public override void CheckInput()
    {
        var newMode = ThighMode.None;

        if (inputConfiguration[Hotkey.DragUpperBone].IsPressed())
            newMode = ThighMode.Rotate;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(ThighGizmoController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(ThighMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(ThighGizmoController controller, ThighMode mode) =>
        controller.CurrentMode = mode switch
        {
            ThighMode.None => controller.None,
            ThighMode.Rotate => controller.Rotate,
            _ => controller.None,
        };
}
