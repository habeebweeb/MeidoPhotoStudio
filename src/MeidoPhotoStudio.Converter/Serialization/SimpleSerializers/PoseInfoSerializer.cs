using MeidoPhotoStudio.Plugin;

namespace MeidoPhotoStudio.Converter.Serialization;

public class PoseInfoSerializer : SimpleSerializer<PoseInfo>
{
    private const short Version = 1;

    public override void Serialize(PoseInfo obj, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        writer.Write(obj.PoseGroup);
        writer.Write(obj.Pose);
        writer.Write(obj.CustomPose);
    }

    public override PoseInfo Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        return new(reader.ReadString(), reader.ReadString(), reader.ReadBoolean());
    }
}
