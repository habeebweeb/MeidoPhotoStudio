using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class PoseInfoSerializer : SimpleSerializer<PoseInfo>
    {
        private const short version = 1;

        public override void Serialize(PoseInfo obj, BinaryWriter writer)
        {
            writer.WriteVersion(version);

            writer.Write(obj.PoseGroup);
            writer.Write(obj.Pose);
            writer.Write(obj.CustomPose);
        }

        public override PoseInfo Deserialize(BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            return new PoseInfo(reader.ReadString(), reader.ReadString(), reader.ReadBoolean());
        }
    }
}
