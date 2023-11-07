using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class LightPropertiesSchemaBuilder : ISchemaBuilder<LightPropertiesSchema, LightProperties>
{
    public LightPropertiesSchema Build(LightProperties lightProperties) =>
        new()
        {
            Rotation = lightProperties.Rotation,
            Intensity = lightProperties.Intensity,
            Range = lightProperties.Range,
            SpotAngle = lightProperties.SpotAngle,
            ShadowStrength = lightProperties.ShadowStrength,
            Colour = lightProperties.Colour,
        };
}
