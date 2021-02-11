using System.IO;

namespace MeidoPhotoStudio.Plugin
{
    public class VignetteEffectSerializer : Serializer<VignetteEffectManager>
    {
        private const short version = 1;

        public override void Serialize(VignetteEffectManager manager, BinaryWriter writer)
        {
            writer.Write(VignetteEffectManager.header);
            writer.WriteVersion(version);

            writer.Write(manager.Active);
            writer.Write(manager.Intensity);
            writer.Write(manager.Blur);
            writer.Write(manager.BlurSpread);
            writer.Write(manager.ChromaticAberration);
        }

        public override void Deserialize(VignetteEffectManager manager, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var active = reader.ReadBoolean();
            manager.Intensity = reader.ReadSingle();
            manager.Blur = reader.ReadSingle();
            manager.BlurSpread = reader.ReadSingle();
            manager.ChromaticAberration = reader.ReadSingle();

            manager.SetEffectActive(active);
        }
    }
}
