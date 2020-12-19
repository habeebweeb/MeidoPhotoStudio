using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class CameraInfoSerializer : Serializer<CameraInfo>
    {
        private const short version = 1;

        public override void Serialize(CameraInfo info, BinaryWriter writer)
        {
            writer.WriteVersion(version);

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
}
