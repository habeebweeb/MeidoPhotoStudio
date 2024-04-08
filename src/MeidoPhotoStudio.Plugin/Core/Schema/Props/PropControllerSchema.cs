namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PropControllerSchema
{
    public const short SchemaVersion = 1;

    public PropControllerSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public TransformSchema Transform { get; init; }

    public IPropModelSchema PropModel { get; init; }

    public bool ShadowCasting { get; init; }

    public bool Visible { get; init; }

    public PropShapeKeySchema ShapeKeys { get; init; }
}
