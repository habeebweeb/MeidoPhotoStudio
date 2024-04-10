using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core.Camera;

/// <summary>Camera manager input handler.</summary>
public class CameraInputHandler(
    CameraController cameraController,
    CameraSpeedController cameraSpeedController,
    CameraSaveSlotController cameraSaveSlotController,
    InputConfiguration inputConfiguration)
    : IInputHandler
{
    private readonly CameraController cameraController = cameraController
        ?? throw new ArgumentNullException(nameof(cameraController));

    private readonly CameraSpeedController cameraSpeedController = cameraSpeedController
        ?? throw new ArgumentNullException(nameof(cameraSpeedController));

    private readonly CameraSaveSlotController cameraSaveSlotController = cameraSaveSlotController
        ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));

    private readonly InputConfiguration inputConfiguration = inputConfiguration
        ?? throw new ArgumentNullException(nameof(inputConfiguration));

    public bool Active { get; } = true;

    public void CheckInput()
    {
        if (inputConfiguration[Shortcut.SaveCamera].IsDown())
            cameraSaveSlotController.SaveTemporaryCameraInfo();
        else if (inputConfiguration[Shortcut.LoadCamera].IsDown())
            cameraSaveSlotController.LoadTemporaryCameraInfo();
        else if (inputConfiguration[Shortcut.ToggleCamera1].IsDown())
            cameraSaveSlotController.CurrentCameraSlot = 0;
        else if (inputConfiguration[Shortcut.ToggleCamera2].IsDown())
            cameraSaveSlotController.CurrentCameraSlot = 1;
        else if (inputConfiguration[Shortcut.ToggleCamera3].IsDown())
            cameraSaveSlotController.CurrentCameraSlot = 2;
        else if (inputConfiguration[Shortcut.ToggleCamera4].IsDown())
            cameraSaveSlotController.CurrentCameraSlot = 3;
        else if (inputConfiguration[Shortcut.ToggleCamera5].IsDown())
            cameraSaveSlotController.CurrentCameraSlot = 4;
        else if (inputConfiguration[Shortcut.ResetCamera].IsDown())
            cameraController.ResetCamera();
        else if (inputConfiguration[Hotkey.FastCamera].IsPressed())
            cameraSpeedController.ApplyFastSpeed();
        else if (inputConfiguration[Hotkey.SlowCamera].IsPressed())
            cameraSpeedController.ApplySlowSpeed();
        else
            cameraSpeedController.ApplyDefaultSpeed();
    }
}
