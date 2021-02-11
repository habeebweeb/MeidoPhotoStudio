using System.IO;

namespace MeidoPhotoStudio.Plugin
{
    public class AttachPointInfoSerializer : SimpleSerializer<AttachPointInfo>
    {
        private const short version = 1;

        public override void Serialize(AttachPointInfo info, BinaryWriter writer)
        {
            writer.WriteVersion(version);

            writer.Write((int) info.AttachPoint);
            writer.Write(info.MaidIndex);
        }

        public override AttachPointInfo Deserialize(BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var attachPoint = (AttachPoint) reader.ReadInt32();
            var maidIndex = reader.ReadInt32();

            return new AttachPointInfo(attachPoint, string.Empty, maidIndex);
        }
    }
}
