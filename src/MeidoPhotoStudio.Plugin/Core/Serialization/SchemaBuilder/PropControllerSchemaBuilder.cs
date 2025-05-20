using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropControllerSchemaBuilder(
    ISchemaBuilder<IPropModelSchema, IPropModel> propModelSchemaBuilder,
    ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder,
    ISchemaBuilder<PropShapeKeySchema, ShapeKeyController> propShapeKeySchemaBuilder)
    : ISchemaBuilder<PropControllerSchema, PropController>
{
    private readonly ISchemaBuilder<IPropModelSchema, IPropModel> propModelSchemaBuilder = propModelSchemaBuilder
        ?? throw new ArgumentNullException(nameof(propModelSchemaBuilder));

    private readonly ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder = transformSchemaBuilder
        ?? throw new ArgumentNullException(nameof(transformSchemaBuilder));

    private readonly ISchemaBuilder<PropShapeKeySchema, ShapeKeyController> propShapeKeySchemaBuilder = propShapeKeySchemaBuilder
        ?? throw new ArgumentNullException(nameof(propShapeKeySchemaBuilder));

    public PropControllerSchema Build(PropController value) =>
        new()
        {
            Transform = transformSchemaBuilder.Build(value.GameObject.transform),
            PropModel = propModelSchemaBuilder.Build(value.PropModel),
            ShadowCasting = value.ShadowCasting,
            Visible = value.Visible,
            ShapeKeys = propShapeKeySchemaBuilder.Build(value.ShapeKeyController),
        };
}
