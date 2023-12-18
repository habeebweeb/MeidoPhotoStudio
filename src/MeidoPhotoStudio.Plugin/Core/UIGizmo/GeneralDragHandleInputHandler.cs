using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public class GeneralDragHandleInputHandler : DragHandleInputHandler<GeneralDragHandleController>
{
    private GeneralDragHandleMode currentDragHandleMode;

    public GeneralDragHandleInputHandler(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    private enum GeneralDragHandleMode
    {
        None,
        Select,
        Delete,
        MoveWorldXZ,
        MoveWorldY,
        RotateWorldY,
        RotateLocalY,
        RotateLocalXZ,
        Scale,
    }

    public override void CheckInput()
    {
        var newMode = GeneralDragHandleMode.None;

        if (inputConfiguration[Hotkey.Select].IsPressed())
            newMode = GeneralDragHandleMode.Select;
        else if (inputConfiguration[Hotkey.Delete].IsPressed())
            newMode = GeneralDragHandleMode.Delete;
        else if (inputConfiguration[Hotkey.MoveWorldXZ].IsPressed())
            newMode = GeneralDragHandleMode.MoveWorldXZ;
        else if (inputConfiguration[Hotkey.MoveWorldY].IsPressed())
            newMode = GeneralDragHandleMode.MoveWorldY;
        else if (inputConfiguration[Hotkey.RotateWorldY].IsPressed())
            newMode = GeneralDragHandleMode.RotateWorldY;
        else if (inputConfiguration[Hotkey.RotateLocalY].IsPressed())
            newMode = GeneralDragHandleMode.RotateLocalY;
        else if (inputConfiguration[Hotkey.RotateLocalXZ].IsPressed())
            newMode = GeneralDragHandleMode.RotateLocalXZ;
        else if (inputConfiguration[Hotkey.Scale].IsPressed())
            newMode = GeneralDragHandleMode.Scale;

        UpdateDragHandleMode(newMode);
    }

    protected override void OnControllerAdded(GeneralDragHandleController controller) =>
        ApplyDragHandleMode(controller, currentDragHandleMode);

    private void UpdateDragHandleMode(GeneralDragHandleMode newMode)
    {
        if (newMode == currentDragHandleMode)
            return;

        currentDragHandleMode = newMode;

        foreach (var controller in this)
            ApplyDragHandleMode(controller, currentDragHandleMode);
    }

    private void ApplyDragHandleMode(GeneralDragHandleController controller, GeneralDragHandleMode mode) =>
        controller.CurrentMode = mode switch
        {
            GeneralDragHandleMode.None => controller.None,
            GeneralDragHandleMode.Select => controller.Select,
            GeneralDragHandleMode.Delete => controller.Delete,
            GeneralDragHandleMode.MoveWorldXZ => controller.MoveWorldXZ,
            GeneralDragHandleMode.MoveWorldY => controller.MoveWorldY,
            GeneralDragHandleMode.RotateWorldY => controller.RotateWorldY,
            GeneralDragHandleMode.RotateLocalY => controller.RotateLocalY,
            GeneralDragHandleMode.RotateLocalXZ => controller.RotateLocalXZ,
            GeneralDragHandleMode.Scale => controller.Scale,
            _ => controller.None,
        };
}
