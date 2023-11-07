using MeidoPhotoStudio.Plugin.Core.Camera;

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class CameraExtensions
{
    public static void StopSpin(this CameraMain camera)
    {
        var uoCamera = camera.GetComponent<UltimateOrbitCamera>();

        uoCamera.xVelocity = 0f;
        uoCamera.yVelocity = 0f;
    }

    public static void StopMovement(this CameraMain camera) =>
        camera.SetTargetPos(camera.GetTargetPos());

    public static void StopAll(this CameraMain camera)
    {
        camera.StopSpin();
        camera.StopMovement();
    }

    public static void ForceCalcNearClip(this CameraMain camera)
    {
        camera.StopAllCoroutines();
        camera.m_bCalcNearClip = false;
        camera.camera.nearClipPlane = 0.01f;
    }

    public static void ResetCalcNearClip(this CameraMain camera)
    {
        if (camera.m_bCalcNearClip)
            return;

        camera.StopAllCoroutines();
        camera.m_bCalcNearClip = true;
        camera.Start();
    }

    public static CameraInfo GetCameraInfo(this CameraMain camera) =>
        new(
            camera.GetTargetPos(),
            camera.transform.rotation,
            camera.GetDistance(),
            camera.camera.fieldOfView);

    public static void ApplyCameraInfo(this CameraMain camera, CameraInfo cameraInfo)
    {
        camera.SetTargetPos(cameraInfo.TargetPos);
        camera.SetDistance(cameraInfo.Distance);

        var cameraEuler = cameraInfo.Angle.eulerAngles;

        camera.SetAroundAngle(new(cameraEuler.y, cameraEuler.x));
        camera.transform.rotation = cameraInfo.Angle;
        camera.camera.fieldOfView = cameraInfo.FOV;

        camera.StopAll();
    }
}
