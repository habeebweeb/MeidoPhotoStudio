using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DepthOfFieldEffectSerializer : Serializer<DepthOfFieldEffectManager>
    {
        private const short version = 1;

        public override void Serialize(DepthOfFieldEffectManager effect, BinaryWriter writer)
        {
            writer.Write(DepthOfFieldEffectManager.header);
            writer.WriteVersion(version);

            writer.Write(effect.Active);
            writer.Write(effect.FocalLength);
            writer.Write(effect.FocalSize);
            writer.Write(effect.Aperture);
            writer.Write(effect.MaxBlurSize);
            writer.Write(effect.VisualizeFocus);
        }

        public override void Deserialize(DepthOfFieldEffectManager effect, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var active = reader.ReadBoolean();
            effect.FocalLength = reader.ReadSingle();
            effect.FocalSize = reader.ReadSingle();
            effect.Aperture = reader.ReadSingle();
            effect.MaxBlurSize = reader.ReadSingle();
            effect.VisualizeFocus = reader.ReadBoolean();

            effect.SetEffectActive(active);
        }
    }
}
