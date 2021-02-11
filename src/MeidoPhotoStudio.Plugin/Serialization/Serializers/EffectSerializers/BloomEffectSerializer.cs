using System.IO;

namespace MeidoPhotoStudio.Plugin
{
    public class BloomEffectSerializer : Serializer<BloomEffectManager>
    {
        private const short version = 1;

        public override void Serialize(BloomEffectManager effect, BinaryWriter writer)
        {
            writer.Write(BloomEffectManager.header);
            writer.WriteVersion(version);

            writer.Write(effect.Active);
            writer.Write(effect.BloomValue);
            writer.Write(effect.BlurIterations);
            writer.Write(effect.BloomThresholdColour);
            writer.Write(effect.BloomHDR);
        }

        public override void Deserialize(BloomEffectManager effect, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var active = reader.ReadBoolean();
            effect.BloomValue = reader.ReadSingle();
            effect.BlurIterations = reader.ReadInt32();
            effect.BloomThresholdColour = reader.ReadColour();
            effect.BloomHDR = reader.ReadBoolean();

            effect.SetEffectActive(active);
        }
    }
}
