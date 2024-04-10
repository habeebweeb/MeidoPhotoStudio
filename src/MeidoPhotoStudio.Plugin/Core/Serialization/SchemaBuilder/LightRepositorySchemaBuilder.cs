using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class LightRepositorySchemaBuilder(
    LightRepository lightRepository, ISchemaBuilder<LightSchema, LightController> lightSchemaBuilder)
    : ISceneSchemaAspectBuilder<LightRepositorySchema>
{
    private readonly LightRepository lightRepository = lightRepository
        ?? throw new ArgumentNullException(nameof(lightRepository));

    private readonly ISchemaBuilder<LightSchema, LightController> lightSchemaBuilder = lightSchemaBuilder
        ?? throw new ArgumentNullException(nameof(lightSchemaBuilder));

    public LightRepositorySchema Build() =>
        new()
        {
            Lights = lightRepository.Select(lightSchemaBuilder.Build).ToList(),
        };
}
