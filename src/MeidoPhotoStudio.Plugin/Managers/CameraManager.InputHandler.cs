using System;

using MeidoPhotoStudio.Plugin.Service.Input;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Camera manager input handler.</summary>
public partial class CameraManager
{
    public class InputHandler : IInputHandler
    {
        private readonly CameraManager cameraManager;

        static InputHandler()
        {
            InputManager.Register(MpsKey.CameraLayer, KeyCode.Q, "Camera control layer");
            InputManager.Register(MpsKey.CameraSave, KeyCode.S, "Save camera transform");
            InputManager.Register(MpsKey.CameraLoad, KeyCode.A, "Load camera transform");
            InputManager.Register(MpsKey.CameraReset, KeyCode.R, "Reset camera transform");
        }

        public InputHandler(CameraManager cameraManager) =>
            this.cameraManager = cameraManager ?? throw new ArgumentNullException(nameof(cameraManager));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (InputManager.GetKey(MpsKey.CameraLayer))
            {
                if (InputManager.GetKeyDown(MpsKey.CameraSave))
                    cameraManager.SaveTempCamera();
                else if (InputManager.GetKeyDown(MpsKey.CameraLoad))
                    cameraManager.LoadCameraInfo(cameraManager.tempCameraInfo);
                else if (InputManager.GetKeyDown(MpsKey.CameraReset))
                    cameraManager.ResetCamera();

                for (var i = 0; i < cameraManager.CameraCount; i++)
                    if (i != cameraManager.CurrentCameraIndex && UnityEngine.Input.GetKeyDown(AlphaOne + i))
                        cameraManager.CurrentCameraIndex = i;
            }

            if (InputManager.Shift)
            {
                UltimateOrbitCamera.moveSpeed = CameraFastMoveSpeed;
                UltimateOrbitCamera.zoomSpeed = CameraFastZoomSpeed;
            }
            else if (InputManager.Control)
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
