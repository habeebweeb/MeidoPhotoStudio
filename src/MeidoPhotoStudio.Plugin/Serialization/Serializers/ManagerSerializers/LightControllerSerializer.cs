using System.IO;

using MeidoPhotoStudio.Plugin.Core.Lighting;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class LightControllerSerializer : Serializer<LightController>
{
    private const short Version = 1;

    private static SimpleSerializer<LightProperties> LightPropertiesSerializer =>
        Serialization.GetSimple<LightProperties>();

    public override void Serialize(LightController lightController, BinaryWriter writer)
    {
        writer.Write(Version);

        LightPropertiesSerializer.Serialize(lightController[LightType.Directional], writer);
        LightPropertiesSerializer.Serialize(lightController[LightType.Spot], writer);
        LightPropertiesSerializer.Serialize(lightController[LightType.Point], writer);

        writer.Write(lightController.Position);
        writer.Write((int)lightController.Type);
        writer.Write(false);
        writer.Write(lightController.Enabled);
    }

    public override void Deserialize(LightController lightController, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var directionalLightProperties = LightPropertiesSerializer.Deserialize(reader, metadata);
        var spotLightProperties = LightPropertiesSerializer.Deserialize(reader, metadata);
        var pointLightProperties = LightPropertiesSerializer.Deserialize(reader, metadata);

        var lightPosition = reader.ReadVector3();
        var lightType = MPSLightTypeToLightType(reader.ReadInt32());

        // IsColourMode
        _ = reader.ReadBoolean();
        var lightEnabled = !reader.ReadBoolean();

        lightController.Position = lightPosition;
        lightController.Type = lightType;
        lightController.Enabled = lightEnabled;

        lightController[LightType.Directional] = directionalLightProperties;
        lightController[LightType.Spot] = spotLightProperties;
        lightController[LightType.Point] = pointLightProperties;

        static LightType MPSLightTypeToLightType(int value) =>
            value switch
            {
                0 => LightType.Directional,
                1 => LightType.Spot,
                2 => LightType.Point,
                _ => LightType.Directional,
            };
    }
}
