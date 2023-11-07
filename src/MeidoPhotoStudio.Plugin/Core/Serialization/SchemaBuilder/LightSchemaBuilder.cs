using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class LightSchemaBuilder : ISchemaBuilder<LightSchema, LightController>
{
    private readonly ISchemaBuilder<LightPropertiesSchema, LightProperties> lightPropertiesSchemaBuilder;

    public LightSchemaBuilder(ISchemaBuilder<LightPropertiesSchema, LightProperties> lightPropertiesSchemaBuilder) =>
        this.lightPropertiesSchemaBuilder = lightPropertiesSchemaBuilder ?? throw new System.ArgumentNullException(nameof(lightPropertiesSchemaBuilder));

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
