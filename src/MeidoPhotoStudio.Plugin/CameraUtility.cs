namespace MeidoPhotoStudio.Plugin;

public static class CameraUtility
{
    public static CameraMain MainCamera =>
        GameMain.Instance.MainCamera;

    public static UltimateOrbitCamera UOCamera { get; } =
        GameMain.Instance.MainCamera.GetComponent<UltimateOrbitCamera>();

    public static void StopSpin()
    {
        Utility.SetFieldValue(UOCamera, "xVelocity", 0f);
        Utility.SetFieldValue(UOCamera, "yVelocity", 0f);
    }

    public static void StopMovement() =>
        MainCamera.SetTargetPos(MainCamera.GetTargetPos());

    public static void StopAll()
    {
        StopSpin();
        StopMovement();
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
}
