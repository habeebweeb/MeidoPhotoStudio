namespace MeidoPhotoStudio.Plugin.Core.Camera;

public readonly record struct CameraInfo(Vector3 TargetPos, Quaternion Angle, float Distance, float FOV);
