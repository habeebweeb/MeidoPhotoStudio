using System.IO;

using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Converter.Serialization;

public class AttachPointInfoSerializer : SimpleSerializer<AttachPointInfo>
{
    private const short Version = 1;

    public override void Serialize(AttachPointInfo info, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        writer.Write((int)info.AttachPoint);
        writer.Write(info.MaidIndex);
    }

    public override AttachPointInfo Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var attachPoint = (AttachPoint)reader.ReadInt32();
        var maidIndex = reader.ReadInt32();

        return new(attachPoint, string.Empty, maidIndex);
    }
}
