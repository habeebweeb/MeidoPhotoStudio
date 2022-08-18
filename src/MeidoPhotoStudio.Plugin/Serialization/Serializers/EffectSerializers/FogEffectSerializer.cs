using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class FogEffectSerializer : Serializer<FogEffectManager>
{
    private const short Version = 1;

    public override void Serialize(FogEffectManager effect, BinaryWriter writer)
    {
        writer.Write(FogEffectManager.Header);
        writer.WriteVersion(Version);

        writer.Write(effect.Active);
        writer.Write(effect.Distance);
        writer.Write(effect.Density);
        writer.Write(effect.HeightScale);
        writer.Write(effect.Height);
        writer.WriteColour(effect.FogColour);
    }

    public override void Deserialize(FogEffectManager effect, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var active = reader.ReadBoolean();

        effect.Distance = reader.ReadSingle();
        effect.Density = reader.ReadSingle();
        effect.HeightScale = reader.ReadSingle();
        effect.Height = reader.ReadSingle();
        effect.FogColour = reader.ReadColour();

        effect.SetEffectActive(active);
    }
}
