namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public interface ISerializable
    {
        void Serialize(System.IO.BinaryWriter binaryWriter);
        void Deserialize(System.IO.BinaryReader binaryReader);
    }
}
