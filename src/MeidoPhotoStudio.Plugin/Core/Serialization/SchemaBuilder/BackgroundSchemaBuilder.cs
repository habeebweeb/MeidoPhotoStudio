using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BackgroundSchemaBuilder(
    BackgroundService backgroundService,
    ISchemaBuilder<BackgroundModelSchema, BackgroundModel> backgroundModelSchemaBuilder,
    ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder)
    : ISceneSchemaAspectBuilder<BackgroundSchema>
{
    private readonly BackgroundService backgroundService = backgroundService
        ?? throw new ArgumentNullException(nameof(backgroundService));

    private readonly ISchemaBuilder<BackgroundModelSchema, BackgroundModel> backgroundModelSchemaBuilder = backgroundModelSchemaBuilder
        ?? throw new ArgumentNullException(nameof(backgroundModelSchemaBuilder));

    private readonly ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder = transformSchemaBuilder
        ?? throw new ArgumentNullException(nameof(transformSchemaBuilder));

    public BackgroundSchema Build() =>
        new()
        {
            Background = backgroundModelSchemaBuilder.Build(backgroundService.CurrentBackground),
            Transform = transformSchemaBuilder.Build(backgroundService.BackgroundTransform),
            Colour = backgroundService.BackgroundColour,
        };
}
