using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class LowerLimbDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<LowerLimbDragHandleController>(inputConfiguration),
    IDragHandleInputHandler<ICharacterDragHandleController>
{
    private LowerLimbMode currentDragHandleMode;

    private enum LowerLimbMode
    {
        Drag,
        Constrain,
        Rotate,
        RotateAlternate,
        RotateBone,
        Ignore,
    }

    void IDragHandleInputHandler<ICharacterDragHandleController>.AddController(ICharacterDragHandleController controller) =>
        AddController((LowerLimbDragHandleController)controller);

    void IDragHandleInputHandler<ICharacterDragHandleController>.RemoveController(ICharacterDragHandleController controller) =>
        RemoveController((LowerLimbDragHandleController)controller);

    public override void CheckInput()
    {
        var newMode = LowerLimbMode.Drag;

        if (inputConfiguration[Hotkey.DragLowerLimb].IsPressed())
            newMode = LowerLimbMode.Constrain;
        else if (inputConfiguration[Hotkey.DragLowerBone].IsPressed())
            newMode = LowerLimbMode.RotateBone;
        else if (inputConfiguration[Hotkey.RotateBody].IsPressed())
            newMode = LowerLimbMode.Rotate;
        else if (inputConfiguration[Hotkey.RotateBodyAlternate].IsPressed())
            newMode = LowerLimbMode.RotateAlternate;
        else if (OtherKeyPressed())
            newMode = LowerLimbMode.Ignore;

        UpdateDragHandleMode(newMode);

        bool OtherKeyPressed() =>
            inputConfiguration.KeyPool.Any(UnityEngine.Input.GetKey);
    }

    protected override void OnControllerAdded(LowerLimbDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentDragHandleMode);

    private void UpdateDragHandleMode(LowerLimbMode newMode)
    {
        if (newMode == currentDragHandleMode)
            return;

        currentDragHandleMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentDragHandleMode);
    }

    private void ApplyDragHandleMode(LowerLimbDragHandleController controller, LowerLimbMode mode) =>
        controller.CurrentMode = mode switch
        {
            LowerLimbMode.Drag => controller.Drag,
            LowerLimbMode.Constrain => controller.Constrained,
            LowerLimbMode.RotateBone or LowerLimbMode.Rotate => controller.Rotate,
            LowerLimbMode.RotateAlternate => controller.RotateAlternate,
            LowerLimbMode.Ignore => controller.Ignore,
            _ => controller.Drag,
        };
}
