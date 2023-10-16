using System.IO;

using MeidoPhotoStudio.Plugin.Core.Camera;

namespace MeidoPhotoStudio.Plugin;

public class CameraInfoSerializer : SimpleSerializer<CameraInfo>
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

    public override CameraInfo Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        return new(
            reader.ReadVector3(),
            reader.ReadQuaternion(),
            reader.ReadSingle(),
            reader.ReadSingle());
    }
}
