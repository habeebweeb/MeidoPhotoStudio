using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class PelvisDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<PelvisDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private PelvisMode currentMode;

    private enum PelvisMode
    {
        None,
        Rotate,
        RotateAlternate,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((PelvisDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((PelvisDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = PelvisMode.None;

        if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            newMode = PelvisMode.Rotate;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            newMode = PelvisMode.RotateAlternate;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(PelvisDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentMode);

    private void UpdateDragHandleMode(PelvisMode newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentMode);
    }

    private void ApplyDragHandleMode(PelvisDragHandleController controller, PelvisMode mode) =>
        controller.CurrentMode = mode switch
        {
            PelvisMode.None => controller.None,
            PelvisMode.Rotate => controller.Rotate,
            PelvisMode.RotateAlternate => controller.RotateAlternate,
            _ => controller.None,
        };
}
