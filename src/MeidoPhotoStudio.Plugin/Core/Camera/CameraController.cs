using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Camera;

public class CameraController : IActivateable
{
    private CameraInfo startingCameraSettings;

    public event EventHandler CameraChange;

    private static CameraMain MainCamera =>
        GameMain.Instance.MainCamera;

    public void ApplyCameraInfo(CameraInfo cameraInfo)
    {
        MainCamera.ApplyCameraInfo(cameraInfo);

        CameraChange?.Invoke(this, EventArgs.Empty);
    }

    public void ResetCamera()
    {
        MainCamera.Reset(CameraMain.CameraType.Target, true);
        MainCamera.SetTargetPos(new(0f, 0.9f, 0f));
        MainCamera.SetDistance(3f);

        CameraChange?.Invoke(this, EventArgs.Empty);
    }

    void IActivateable.Activate()
    {
        if (MainCamera.m_UOCamera)
            MainCamera.m_UOCamera.enabled = true;

        MainCamera.ForceCalcNearClip();

        startingCameraSettings = MainCamera.GetCameraInfo();
    }

    void IActivateable.Deactivate()
    {
        MainCamera.camera.backgroundColor = Color.black;

        MainCamera.ResetCalcNearClip();

        MainCamera.ApplyCameraInfo(startingCameraSettings);
    }
}
