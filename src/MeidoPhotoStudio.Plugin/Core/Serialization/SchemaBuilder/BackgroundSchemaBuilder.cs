using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BackgroundSchemaBuilder : ISceneSchemaAspectBuilder<BackgroundSchema>
{
    private readonly BackgroundService backgroundService;
    private readonly ISchemaBuilder<BackgroundModelSchema, BackgroundModel> backgroundModelSchemaBuilder;
    private readonly ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder;

    public BackgroundSchemaBuilder(
        BackgroundService backgroundService,
        ISchemaBuilder<BackgroundModelSchema, BackgroundModel> backgroundModelSchemaBuilder,
        ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder)
    {
        this.backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
        this.backgroundModelSchemaBuilder = backgroundModelSchemaBuilder ?? throw new ArgumentNullException(nameof(backgroundModelSchemaBuilder));
        this.transformSchemaBuilder = transformSchemaBuilder ?? throw new ArgumentNullException(nameof(transformSchemaBuilder));
    }

    public BackgroundSchema Build() =>
        new()
        {
            Background = backgroundModelSchemaBuilder.Build(backgroundService.CurrentBackground),
            Transform = transformSchemaBuilder.Build(backgroundService.BackgroundTransform),
            Colour = backgroundService.BackgroundColour,
        };
}
