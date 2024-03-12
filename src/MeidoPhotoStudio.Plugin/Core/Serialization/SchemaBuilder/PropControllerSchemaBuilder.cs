using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropControllerSchemaBuilder : ISchemaBuilder<PropControllerSchema, PropController>
{
    private readonly ISchemaBuilder<IPropModelSchema, IPropModel> propModelSchemaBuilder;
    private readonly ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder;

    public PropControllerSchemaBuilder(
        ISchemaBuilder<IPropModelSchema, IPropModel> propModelSchemaBuilder,
        ISchemaBuilder<TransformSchema, Transform> transformSchemaBuilder)
    {
        this.propModelSchemaBuilder = propModelSchemaBuilder ?? throw new System.ArgumentNullException(nameof(propModelSchemaBuilder));
        this.transformSchemaBuilder = transformSchemaBuilder ?? throw new System.ArgumentNullException(nameof(transformSchemaBuilder));
    }

    public PropControllerSchema Build(PropController value) =>
        new()
        {
            Transform = transformSchemaBuilder.Build(value.GameObject.transform),
            PropModel = propModelSchemaBuilder.Build(value.PropModel),
            ShadowCasting = value.ShadowCasting,
            Visible = value.Visible,
        };
}
