using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FloorHeightDragHandleInputHandler(InputConfiguration inputConfiguration)
    : DragHandleInputHandler<FloorHeightDragHandleController>(inputConfiguration)
{
    private Mode currentMode;

    private enum Mode
    {
        Normal,
        Ignore,
    }

    public override void CheckInput()
    {
        var newMode = inputConfiguration.KeyPool.Any(Input.GetKey) ? Mode.Ignore : Mode.Normal;

        if (newMode == currentMode)
            return;

        currentMode = newMode;

        foreach (var controller in this)
            controller.CurrentMode = currentMode switch
            {
                Mode.Normal => controller.AdjustMode,
                Mode.Ignore => controller.Ignore,
                _ => controller.AdjustMode,
            };
    }
}
