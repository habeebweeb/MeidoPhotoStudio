using System;

using MeidoPhotoStudio.Plugin.Core;

using UnityEngine;

using Input = MeidoPhotoStudio.Plugin.InputManager;
using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin;

public class CameraManager : IManager
{
    public const string Header = "CAMERA";

    private const float CameraFastMoveSpeed = 0.1f;
    private const float CameraFastZoomSpeed = 3f;
    private const float CameraSlowMoveSpeed = 0.004f;
    private const float CameraSlowZoomSpeed = 0.1f;
    private const KeyCode AlphaOne = KeyCode.Alpha1;

    private static readonly CameraMain MainCamera = CameraUtility.MainCamera;
    private static readonly UltimateOrbitCamera UltimateOrbitCamera = CameraUtility.UOCamera;

    private readonly CameraInfo tempCameraInfo = new();
    private readonly CameraInfo[] cameraInfos;

    private float defaultCameraMoveSpeed;
    private float defaultCameraZoomSpeed;
    private Camera subCamera;
    private int currentCameraIndex;

    static CameraManager()
    {
        Input.Register(MpsKey.CameraLayer, KeyCode.Q, "Camera control layer");
        Input.Register(MpsKey.CameraSave, KeyCode.S, "Save camera transform");
        Input.Register(MpsKey.CameraLoad, KeyCode.A, "Load camera transform");
        Input.Register(MpsKey.CameraReset, KeyCode.R, "Reset camera transform");
    }

    public CameraManager()
    {
        cameraInfos = new CameraInfo[5];

        for (var i = 0; i < cameraInfos.Length; i++)
            cameraInfos[i] = new();

        Activate();
    }

    public event EventHandler CameraChange;

    public int CameraCount =>
        cameraInfos.Length;

    public int CurrentCameraIndex
    {
        get => currentCameraIndex;
        set
        {
            cameraInfos[currentCameraIndex].UpdateInfo(MainCamera);
            currentCameraIndex = value;
            LoadCameraInfo(cameraInfos[currentCameraIndex]);
        }
    }

    public void Activate()
    {
        UltimateOrbitCamera.enabled = true;

        defaultCameraMoveSpeed = UltimateOrbitCamera.moveSpeed;
        defaultCameraZoomSpeed = UltimateOrbitCamera.zoomSpeed;

        if (!PluginCore.EditMode)
            ResetCamera();

        currentCameraIndex = 0;

        tempCameraInfo.Reset();

        for (var i = 0; i < CameraCount; i++)
            cameraInfos[i].Reset();

        MainCamera.ForceCalcNearClip();

        var subCamGo = new GameObject("subcam");

        subCamera = subCamGo.AddComponent<Camera>();
        subCamera.CopyFrom(MainCamera.camera);
        subCamera.clearFlags = CameraClearFlags.Depth;
        subCamera.cullingMask = 1 << 8;
        subCamera.depth = 1f;
        subCamera.transform.parent = MainCamera.transform;
    }

    public void Deactivate()
    {
        UnityEngine.Object.Destroy(subCamera.gameObject);
        MainCamera.camera.backgroundColor = Color.black;

        UltimateOrbitCamera.moveSpeed = defaultCameraMoveSpeed;
        UltimateOrbitCamera.zoomSpeed = defaultCameraZoomSpeed;

        if (PluginCore.EditMode)
            return;

        MainCamera.Reset(CameraMain.CameraType.Target, true);
        MainCamera.SetTargetPos(new(0.5609447f, 1.380762f, -1.382336f));
        MainCamera.SetDistance(1.6f);
        MainCamera.SetAroundAngle(new(245.5691f, 6.273283f));

        MainCamera.ResetCalcNearClip();
    }

    public void Update()
    {
        if (Input.GetKey(MpsKey.CameraLayer))
        {
            if (Input.GetKeyDown(MpsKey.CameraSave))
                SaveTempCamera();
            else if (Input.GetKeyDown(MpsKey.CameraLoad))
                LoadCameraInfo(tempCameraInfo);
            else if (Input.GetKeyDown(MpsKey.CameraReset))
                ResetCamera();

            for (var i = 0; i < CameraCount; i++)
                if (i != CurrentCameraIndex && UInput.GetKeyDown(AlphaOne + i))
                    CurrentCameraIndex = i;
        }

        subCamera.fieldOfView = MainCamera.camera.fieldOfView;

        if (Input.Shift)
        {
            UltimateOrbitCamera.moveSpeed = CameraFastMoveSpeed;
            UltimateOrbitCamera.zoomSpeed = CameraFastZoomSpeed;
        }
        else if (Input.Control)
        {
            UltimateOrbitCamera.moveSpeed = CameraSlowMoveSpeed;
            UltimateOrbitCamera.zoomSpeed = CameraSlowZoomSpeed;
        }
        else
        {
            UltimateOrbitCamera.moveSpeed = defaultCameraMoveSpeed;
            UltimateOrbitCamera.zoomSpeed = defaultCameraZoomSpeed;
        }
    }

    public void LoadCameraInfo(CameraInfo info)
    {
        info.Apply(MainCamera);
        CameraUtility.StopAll();
        CameraChange?.Invoke(this, EventArgs.Empty);
    }

    private void SaveTempCamera()
    {
        tempCameraInfo.UpdateInfo(MainCamera);
        CameraUtility.StopAll();
    }

    private void ResetCamera()
    {
        MainCamera.Reset(CameraMain.CameraType.Target, true);
        MainCamera.SetTargetPos(new(0f, 0.9f, 0f));
        MainCamera.SetDistance(3f);
        CameraChange?.Invoke(this, EventArgs.Empty);
    }
}
