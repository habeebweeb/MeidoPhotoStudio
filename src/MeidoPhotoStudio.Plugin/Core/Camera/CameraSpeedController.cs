namespace MeidoPhotoStudio.Plugin.Core.Camera;

public class CameraSpeedController
{
    private readonly float fastMoveSpeed = 0.1f;
    private readonly float fastZoomSpeed = 3f;
    private readonly float slowMoveSpeed = 0.004f;
    private readonly float slowZoomSpeed = 0.1f;
    private readonly float defaultMoveSpeed;
    private readonly float defaultZoomSpeed;

    private UltimateOrbitCamera ultimateOrbitCamera;
    private Speed currentCameraSpeed = Speed.Default;

    public CameraSpeedController()
    {
        defaultMoveSpeed = UltimateOrbitCamera.moveSpeed;
        defaultZoomSpeed = UltimateOrbitCamera.zoomSpeed;
    }

    public enum Speed
    {
        Default,
        Fast,
        Slow,
    }

    private UltimateOrbitCamera UltimateOrbitCamera =>
        ultimateOrbitCamera
            ? ultimateOrbitCamera
            : ultimateOrbitCamera = GameMain.Instance.MainCamera.GetComponent<UltimateOrbitCamera>();

    public void Deactivate() =>
        ApplyDefaultSpeed();

    public void ApplyFastSpeed()
    {
        if (currentCameraSpeed is Speed.Fast)
            return;

        currentCameraSpeed = Speed.Fast;

        UltimateOrbitCamera.moveSpeed = fastMoveSpeed;
        UltimateOrbitCamera.zoomSpeed = fastZoomSpeed;
    }

    public void ApplySlowSpeed()
    {
        if (currentCameraSpeed is Speed.Slow)
            return;

        currentCameraSpeed = Speed.Slow;

        UltimateOrbitCamera.moveSpeed = slowMoveSpeed;
        UltimateOrbitCamera.zoomSpeed = slowZoomSpeed;
    }

    public void ApplyDefaultSpeed()
    {
        if (currentCameraSpeed is Speed.Default)
            return;

        currentCameraSpeed = Speed.Default;

        UltimateOrbitCamera.moveSpeed = defaultMoveSpeed;
        UltimateOrbitCamera.zoomSpeed = defaultZoomSpeed;
    }
}
