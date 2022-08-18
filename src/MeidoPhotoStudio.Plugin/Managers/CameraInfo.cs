using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class CameraInfo
{
    public CameraInfo() =>
        Reset();

    public Vector3 TargetPos { get; set; }

    public Quaternion Angle { get; set; }

    public float Distance { get; set; }

    public float FOV { get; set; }

    public static CameraInfo FromCamera(CameraMain mainCamera)
    {
        var info = new CameraInfo();

        info.UpdateInfo(mainCamera);

        return info;
    }

    public void Reset()
    {
        TargetPos = new(0f, 0.9f, 0f);
        Angle = Quaternion.Euler(10f, 180f, 0f);
        Distance = 3f;
        FOV = 35f;
    }

    public void UpdateInfo(CameraMain camera)
    {
        TargetPos = camera.GetTargetPos();
        Angle = camera.transform.rotation;
        Distance = camera.GetDistance();
        FOV = camera.camera.fieldOfView;
    }

    public void Apply(CameraMain camera)
    {
        camera.SetTargetPos(TargetPos);
        camera.SetDistance(Distance);
        camera.transform.rotation = Angle;
        camera.camera.fieldOfView = FOV;
    }
}
