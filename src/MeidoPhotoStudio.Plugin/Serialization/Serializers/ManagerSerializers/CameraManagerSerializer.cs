using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class CameraManagerSerializer : Serializer<CameraManager>
{
    private const short Version = 1;

    private static readonly CameraInfo DummyInfo = new();

    private static Serializer<CameraInfo> InfoSerializer =>
        Serialization.Get<CameraInfo>();

    public override void Serialize(CameraManager manager, BinaryWriter writer)
    {
        writer.Write(CameraManager.Header);
        writer.WriteVersion(Version);

        var cameraInfos = GetCameraInfos(manager);

        cameraInfos[manager.CurrentCameraIndex].UpdateInfo(CameraUtility.MainCamera);

        writer.Write(manager.CurrentCameraIndex);
        writer.Write(manager.CameraCount);

        foreach (var info in cameraInfos)
            InfoSerializer.Serialize(info, writer);

        CameraUtility.StopAll();
    }

    public override void Deserialize(CameraManager manager, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var camera = CameraUtility.MainCamera;

        manager.CurrentCameraIndex = reader.ReadInt32();

        var cameraCount = reader.ReadInt32();
        var cameraInfos = GetCameraInfos(manager);

        for (var i = 0; i < cameraCount; i++)
            InfoSerializer.Deserialize(i >= manager.CameraCount ? DummyInfo : cameraInfos[i], reader, metadata);

        if (metadata.Environment)
            return;

        cameraInfos[manager.CurrentCameraIndex].Apply(camera);

        CameraUtility.StopAll();
    }

    private static CameraInfo[] GetCameraInfos(CameraManager manager) =>
        Utility.GetFieldValue<CameraManager, CameraInfo[]>(manager, "cameraInfos");
}
