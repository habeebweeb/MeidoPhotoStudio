namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PropShapeKeySchema(short version = PropShapeKeySchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public Dictionary<string, float> BlendValues { get; init; }
}
