using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class LightRepositorySchemaBuilder : ISceneSchemaAspectBuilder<LightRepositorySchema>
{
    private readonly LightRepository lightRepository;
    private readonly ISchemaBuilder<LightSchema, LightController> lightSchemaBuilder;

    public LightRepositorySchemaBuilder(LightRepository lightRepository, ISchemaBuilder<LightSchema, LightController> lightSchemaBuilder)
    {
        this.lightRepository = lightRepository ?? throw new System.ArgumentNullException(nameof(lightRepository));
        this.lightSchemaBuilder = lightSchemaBuilder ?? throw new System.ArgumentNullException(nameof(lightSchemaBuilder));
    }

    public LightRepositorySchema Build() =>
        new()
        {
            Lights = lightRepository.Select(lightSchemaBuilder.Build).ToList(),
        };
}
