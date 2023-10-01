using System;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Camera manager input handler.</summary>
public partial class CameraManager
{
    public class InputHandler : IInputHandler
    {
        private readonly CameraManager cameraManager;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(CameraManager cameraManager, InputConfiguration inputConfiguration)
        {
            this.cameraManager = cameraManager ?? throw new ArgumentNullException(nameof(cameraManager));
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.SaveCamera].IsDown())
                cameraManager.SaveTempCamera();
            else if (inputConfiguration[Shortcut.LoadCamera].IsDown())
                cameraManager.LoadCameraInfo(cameraManager.tempCameraInfo);
            else if (inputConfiguration[Shortcut.ResetCamera].IsDown())
                cameraManager.ResetCamera();
            else if (inputConfiguration[Shortcut.ToggleCamera1].IsDown())
                cameraManager.CurrentCameraIndex = 0;
            else if (inputConfiguration[Shortcut.ToggleCamera2].IsDown())
                cameraManager.CurrentCameraIndex = 1;
            else if (inputConfiguration[Shortcut.ToggleCamera3].IsDown())
                cameraManager.CurrentCameraIndex = 2;
            else if (inputConfiguration[Shortcut.ToggleCamera4].IsDown())
                cameraManager.CurrentCameraIndex = 3;
            else if (inputConfiguration[Shortcut.ToggleCamera5].IsDown())
                cameraManager.CurrentCameraIndex = 4;

            if (inputConfiguration[Hotkey.FastCamera].IsPressed())
            {
                UltimateOrbitCamera.moveSpeed = CameraFastMoveSpeed;
                UltimateOrbitCamera.zoomSpeed = CameraFastZoomSpeed;
            }
            else if (inputConfiguration[Hotkey.SlowCamera].IsPressed())
            {
                UltimateOrbitCamera.moveSpeed = CameraSlowMoveSpeed;
                UltimateOrbitCamera.zoomSpeed = CameraSlowZoomSpeed;
            }
            else
            {
                UltimateOrbitCamera.moveSpeed = cameraManager.defaultCameraMoveSpeed;
                UltimateOrbitCamera.zoomSpeed = cameraManager.defaultCameraZoomSpeed;
            }
        }
    }
}
