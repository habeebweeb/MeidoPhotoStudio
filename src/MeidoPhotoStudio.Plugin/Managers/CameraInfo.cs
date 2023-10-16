using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Camera;

public readonly struct CameraInfo
{
    public CameraInfo(Vector3 targetPosition, Quaternion angle, float distance, float fov)
    {
        TargetPos = targetPosition;
        Angle = angle;
        Distance = distance;
        FOV = fov;
    }

    public Vector3 TargetPos { get; }

    public Quaternion Angle { get; }

    public float Distance { get; }

    public float FOV { get; }
}
