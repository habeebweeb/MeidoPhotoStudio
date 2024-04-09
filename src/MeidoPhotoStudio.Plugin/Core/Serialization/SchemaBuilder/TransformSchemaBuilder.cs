using MeidoPhotoStudio.Plugin.Core.Schema;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class TransformSchemaBuilder : ISchemaBuilder<TransformSchema, Transform>
{
    public TransformSchema Build(Transform transform) =>
        new()
        {
            Position = transform.position,
            Rotation = transform.rotation,
            LocalPosition = transform.localPosition,
            LocalRotation = transform.localRotation,
            LocalScale = transform.localScale,
        };
}
