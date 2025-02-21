using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class LightsSchemaBuilder(
    LightService lightService, ISchemaBuilder<LightSchema, LightController> lightSchemaBuilder)
    : ISceneSchemaAspectBuilder<LightsSchema>
{
    private readonly LightService lightService = lightService
        ?? throw new ArgumentNullException(nameof(lightService));

    private readonly ISchemaBuilder<LightSchema, LightController> lightSchemaBuilder = lightSchemaBuilder
        ?? throw new ArgumentNullException(nameof(lightSchemaBuilder));

    public LightsSchema Build() =>
        new()
        {
            Lights = lightService.Select(lightSchemaBuilder.Build).ToList(),
        };
}
