namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PropControllerSchema(short version = PropControllerSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public TransformSchema Transform { get; init; }

    public IPropModelSchema PropModel { get; init; }

    public bool ShadowCasting { get; init; }

    public bool Visible { get; init; }

    public PropShapeKeySchema ShapeKeys { get; init; }
}
