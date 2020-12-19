using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public interface ISerializer
    {
        void Serialize(object thing, BinaryWriter writer);
        void Deserialize(object thing, BinaryReader reader, SceneMetadata metadata);
    }
}
