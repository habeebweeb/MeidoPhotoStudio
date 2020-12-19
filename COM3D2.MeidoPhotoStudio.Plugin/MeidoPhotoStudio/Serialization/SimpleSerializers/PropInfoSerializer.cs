using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class PropInfoSerializer : SimpleSerializer<PropInfo>
    {
        private const short version = 1;

        public override void Serialize(PropInfo info, BinaryWriter writer)
        {
            writer.WriteVersion(version);

            writer.Write((int) info.Type);
            writer.WriteNullableString(info.Filename);
            writer.WriteNullableString(info.SubFilename);
            writer.Write(info.MyRoomID);
            writer.WriteNullableString(info.IconFile);
        }

        public override PropInfo Deserialize(BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            return new PropInfo ((PropInfo.PropType) reader.ReadInt32())
            {
                Filename = reader.ReadNullableString(),
                SubFilename = reader.ReadNullableString(),
                MyRoomID = reader.ReadInt32(),
                IconFile = reader.ReadNullableString()
            };
        }
    }
}
