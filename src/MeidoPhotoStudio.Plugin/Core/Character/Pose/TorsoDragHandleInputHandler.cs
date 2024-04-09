using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class TorsoDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<TorsoDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private TorsoMode currentMode;

    private enum TorsoMode
    {
        None,
        Rotate,
        RotateAlternate,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((TorsoDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((TorsoDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = TorsoMode.None;

        if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            newMode = TorsoMode.Rotate;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            newMode = TorsoMode.RotateAlternate;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(TorsoDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(TorsoMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(TorsoDragHandleController controller, TorsoMode mode) =>
        controller.CurrentMode = mode switch
        {
            TorsoMode.None => controller.None,
            TorsoMode.Rotate => controller.Rotate,
            TorsoMode.RotateAlternate => controller.RotateAlternate,
            _ => controller.None,
        };
}
