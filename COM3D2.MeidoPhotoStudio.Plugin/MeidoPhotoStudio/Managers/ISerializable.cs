namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal interface ISerializable
    {
        void Serialize(System.IO.BinaryWriter binaryWriter);
        void Deserialize(System.IO.BinaryReader binaryReader);
    }
}
