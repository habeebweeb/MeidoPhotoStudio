using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class BlurEffectSerializer : Serializer<BlurEffectManager>
    {
        private const short version = 1;

        public override void Serialize(BlurEffectManager effect, BinaryWriter writer)
        {
            writer.Write(BlurEffectManager.header);
            writer.WriteVersion(version);

            writer.Write(effect.Active);
            writer.Write(effect.BlurSize);
        }

        public override void Deserialize(BlurEffectManager effect, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var active = reader.ReadBoolean();
            effect.BlurSize = reader.ReadSingle();

            effect.SetEffectActive(active);
        }
    }
}
