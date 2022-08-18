using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class SepiaToneEffectSerializer : Serializer<SepiaToneEffectManager>
{
    private const short Version = 1;

    public override void Serialize(SepiaToneEffectManager effect, BinaryWriter writer)
    {
        writer.Write(SepiaToneEffectManager.Header);

        writer.WriteVersion(Version);

        writer.Write(effect.Active);
    }

    public override void Deserialize(SepiaToneEffectManager effect, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        effect.SetEffectActive(reader.ReadBoolean());
    }
}
