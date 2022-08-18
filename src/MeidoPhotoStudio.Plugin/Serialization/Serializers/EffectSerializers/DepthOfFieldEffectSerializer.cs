using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class DepthOfFieldEffectSerializer : Serializer<DepthOfFieldEffectManager>
{
    private const short Version = 1;

    public override void Serialize(DepthOfFieldEffectManager effect, BinaryWriter writer)
    {
        writer.Write(DepthOfFieldEffectManager.Header);
        writer.WriteVersion(Version);

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
