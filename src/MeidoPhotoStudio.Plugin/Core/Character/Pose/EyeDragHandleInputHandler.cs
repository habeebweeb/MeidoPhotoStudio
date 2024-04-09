using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class EyeDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<EyeDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private EyeMode currentMode;

    private enum EyeMode
    {
        None,
        RotateEye,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((EyeDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((EyeDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = EyeMode.None;

        if (inputConfiguration[Hotkey.RotateEyesChestAlternate].IsPressed())
            newMode = EyeMode.RotateEye;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(EyeDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(EyeMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(EyeDragHandleController controller, EyeMode mode) =>
        controller.CurrentMode = mode switch
        {
            EyeMode.None => controller.None,
            EyeMode.RotateEye => controller.RotateEye,
            _ => controller.None,
        };
}
