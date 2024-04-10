using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class LightSchemaBuilder(
    ISchemaBuilder<LightPropertiesSchema, LightProperties> lightPropertiesSchemaBuilder)
    : ISchemaBuilder<LightSchema, LightController>
{
    private readonly ISchemaBuilder<LightPropertiesSchema, LightProperties> lightPropertiesSchemaBuilder = lightPropertiesSchemaBuilder
        ?? throw new ArgumentNullException(nameof(lightPropertiesSchemaBuilder));

    public LightSchema Build(LightController lightController) =>
        new()
        {
            DirectionalProperties = lightPropertiesSchemaBuilder.Build(lightController[LightType.Directional]),
            SpotProperties = lightPropertiesSchemaBuilder.Build(lightController[LightType.Spot]),
            PointProperties = lightPropertiesSchemaBuilder.Build(lightController[LightType.Point]),
            Position = lightController.Position,
            Type = lightController.Type,
            Enabled = lightController.Enabled,
        };
}
