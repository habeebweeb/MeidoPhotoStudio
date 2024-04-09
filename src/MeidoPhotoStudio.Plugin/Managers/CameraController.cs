using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Service;

namespace MeidoPhotoStudio.Plugin.Core.Camera;

public class CameraController : IManager
{
    private readonly CustomMaidSceneService customMaidSceneService;

    public CameraController(CustomMaidSceneService customMaidSceneService) =>
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));

    public event EventHandler CameraChange;

    private static CameraMain MainCamera =>
        GameMain.Instance.MainCamera;

    public void Activate()
    {
        MainCamera.m_UOCamera.enabled = true;

        if (customMaidSceneService.OfficeScene)
            ResetCamera();

        MainCamera.ForceCalcNearClip();
    }

    public void Deactivate()
    {
        MainCamera.camera.backgroundColor = Color.black;

        MainCamera.ResetCalcNearClip();

        if (!customMaidSceneService.OfficeScene)
            return;

        ResetCameraForOfficeMode();

        static void ResetCameraForOfficeMode()
        {
            MainCamera.Reset(CameraMain.CameraType.Target, true);
            MainCamera.SetTargetPos(new(0.5609447f, 1.380762f, -1.382336f));
            MainCamera.SetDistance(1.6f);
            MainCamera.SetAroundAngle(new(245.5691f, 6.273283f));
        }
    }

    public void Update()
    {
    }

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
}
