using System.IO;

using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Converter.Serialization;

public class PropInfoSerializer : SimpleSerializer<PropInfo>
{
    private const short Version = 1;

    public override void Serialize(PropInfo info, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        writer.Write((int)info.Type);
        writer.WriteNullableString(info.Filename);
        writer.WriteNullableString(info.SubFilename);
        writer.Write(info.MyRoomID);
        writer.WriteNullableString(info.IconFile);
    }

    public override PropInfo Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        return new PropInfo((PropInfo.PropType)reader.ReadInt32())
        {
            Filename = reader.ReadNullableString(),
            SubFilename = reader.ReadNullableString(),
            MyRoomID = reader.ReadInt32(),
            IconFile = reader.ReadNullableString(),
        };
    }
}
