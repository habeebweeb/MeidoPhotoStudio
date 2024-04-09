using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class UpperLimbDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<UpperLimbDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private UpperLimbMode currentDragHandleMode;

    private enum UpperLimbMode
    {
        Drag,
        Rotate,
        Ignore,
    }

    public override void CheckInput()
    {
        var newMode = UpperLimbMode.Drag;

        if (inputConfiguration[Hotkey.DragUpperBone].IsPressed())
            newMode = UpperLimbMode.Rotate;
        else if (OtherKeyPressed())
            newMode = UpperLimbMode.Ignore;

        UpdateDragHandleMode(newMode);

        bool OtherKeyPressed() =>
            inputConfiguration.KeyPool.Any(UnityEngine.Input.GetKey);
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((UpperLimbDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((UpperLimbDragHandleController)controller);

    protected override void OnControllerAdded(UpperLimbDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentDragHandleMode);

    private void UpdateDragHandleMode(UpperLimbMode newMode)
    {
        if (newMode == currentDragHandleMode)
            return;

        currentDragHandleMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentDragHandleMode);
    }

    private void ApplyDragHandleMode(UpperLimbDragHandleController controller, UpperLimbMode mode) =>
        controller.CurrentMode = mode switch
        {
            UpperLimbMode.Drag => controller.Drag,
            UpperLimbMode.Rotate => controller.Rotate,
            UpperLimbMode.Ignore => controller.Ignore,
            _ => controller.Drag,
        };
}
