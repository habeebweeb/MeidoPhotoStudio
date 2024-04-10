namespace MeidoPhotoStudio.Plugin.Core.Camera;

public readonly struct CameraInfo(Vector3 targetPosition, Quaternion angle, float distance, float fov)
{
    public Vector3 TargetPos { get; } = targetPosition;

    public Quaternion Angle { get; } = angle;

    public float Distance { get; } = distance;

    public float FOV { get; } = fov;
}
