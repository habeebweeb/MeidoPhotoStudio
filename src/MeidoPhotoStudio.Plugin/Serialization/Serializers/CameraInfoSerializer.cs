using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class CameraInfoSerializer : Serializer<CameraInfo>
{
    private const short Version = 1;

    public override void Serialize(CameraInfo info, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        writer.Write(info.TargetPos);
        writer.Write(info.Angle);
        writer.Write(info.Distance);
        writer.Write(info.FOV);
    }

    public override void Deserialize(CameraInfo info, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        info.TargetPos = reader.ReadVector3();
        info.Angle = reader.ReadQuaternion();
        info.Distance = reader.ReadSingle();
        info.FOV = reader.ReadSingle();
    }
}
