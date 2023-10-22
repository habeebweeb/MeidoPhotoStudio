using System.IO;

using MeidoPhotoStudio.Plugin.Core.Lighting;

namespace MeidoPhotoStudio.Plugin;

public class LightPropertiesSerializer : SimpleSerializer<LightProperties>
{
    private const short Version = 1;

    public override void Serialize(LightProperties lightProperties, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        writer.Write(lightProperties.Rotation);
        writer.Write(lightProperties.Intensity);
        writer.Write(lightProperties.Range);
        writer.Write(lightProperties.SpotAngle);
        writer.Write(lightProperties.ShadowStrength);
        writer.Write(lightProperties.Colour);
    }

    public override LightProperties Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        return new()
        {
            Rotation = reader.ReadQuaternion(),
            Intensity = reader.ReadSingle(),
            Range = reader.ReadSingle(),
            SpotAngle = reader.ReadSingle(),
            ShadowStrength = reader.ReadSingle(),
            Colour = reader.ReadColour(),
        };
    }
}
