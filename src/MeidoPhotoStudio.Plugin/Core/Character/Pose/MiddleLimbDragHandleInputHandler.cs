using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class MiddleLimbDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<MiddleLimbDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private MiddleLimbMode currentDragHandleMode;

    private enum MiddleLimbMode
    {
        Drag,
        Rotate,
        RotateBone,
        Ignore,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((MiddleLimbDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((MiddleLimbDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = MiddleLimbMode.Drag;

        if (inputConfiguration[Hotkey.DragMiddleBone].IsPressed())
            newMode = MiddleLimbMode.RotateBone;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            newMode = MiddleLimbMode.Rotate;
        else if (OtherKeyPressed())
            newMode = MiddleLimbMode.Ignore;

        UpdateDragHandleMode(newMode);

        bool OtherKeyPressed() =>
            inputConfiguration.KeyPool.Any(UnityEngine.Input.GetKey);
    }

    protected override void OnControllerAdded(MiddleLimbDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentDragHandleMode);

    private void UpdateDragHandleMode(MiddleLimbMode newMode)
    {
        if (newMode == currentDragHandleMode)
            return;

        currentDragHandleMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentDragHandleMode);
    }

    private void ApplyDragHandleMode(MiddleLimbDragHandleController controller, MiddleLimbMode mode) =>
        controller.CurrentMode = mode switch
        {
            MiddleLimbMode.Drag => controller.Drag,
            MiddleLimbMode.Rotate => controller.Rotate,
            MiddleLimbMode.RotateBone => controller.RotateBone,
            MiddleLimbMode.Ignore => controller.Ignore,
            _ => controller.Drag,
        };
}
