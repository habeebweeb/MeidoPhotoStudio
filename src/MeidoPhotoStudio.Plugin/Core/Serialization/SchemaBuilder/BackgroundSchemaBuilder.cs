using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BackgroundSchemaBuilder
{
    private readonly BackgroundService backgroundService;
    private readonly ISchemaBuilder<BackgroundModelSchema, BackgroundModel> backgroundModelSchemaBuilder;
    private readonly ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder;

    public BackgroundSchemaBuilder(
        BackgroundService backgroundService,
        ISchemaBuilder<BackgroundModelSchema, BackgroundModel> backgroundModelSchemaBuilder,
        ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder)
    {
        this.backgroundService = backgroundService ?? throw new System.ArgumentNullException(nameof(backgroundService));
        this.backgroundModelSchemaBuilder = backgroundModelSchemaBuilder ?? throw new System.ArgumentNullException(nameof(backgroundModelSchemaBuilder));
        this.transformSchemaBuilder = transformSchemaBuilder ?? throw new System.ArgumentNullException(nameof(transformSchemaBuilder));
    }

    public BackgroundSchema Build() =>
        new()
        {
            Background = backgroundModelSchemaBuilder.Build(backgroundService.CurrentBackground),
            Transform = transformSchemaBuilder.Build(backgroundService.BackgroundTransform),
            Colour = backgroundService.BackgroundColour,
        };
}
