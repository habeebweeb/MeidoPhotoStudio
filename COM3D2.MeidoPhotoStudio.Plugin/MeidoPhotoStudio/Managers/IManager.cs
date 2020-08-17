namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal interface IManager
    {
        void Update();
        void Activate();
        void Deactivate();
        void Serialize(System.IO.BinaryWriter binaryWriter);
        void Deserialize(System.IO.BinaryReader binaryReader);
    }
}
