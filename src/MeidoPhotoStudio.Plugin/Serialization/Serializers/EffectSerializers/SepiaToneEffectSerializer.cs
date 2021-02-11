using System.IO;

namespace MeidoPhotoStudio.Plugin
{
    public class SepiaToneEffectSerializer : Serializer<SepiaToneEffectManger>
    {
        private const short version = 1;

        public override void Serialize(SepiaToneEffectManger effect, BinaryWriter writer)
        {
            writer.Write(SepiaToneEffectManger.header);
            writer.WriteVersion(version);

            writer.Write(effect.Active);
        }

        public override void Deserialize(SepiaToneEffectManger effect, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            effect.SetEffectActive(reader.ReadBoolean());
        }
    }
}
