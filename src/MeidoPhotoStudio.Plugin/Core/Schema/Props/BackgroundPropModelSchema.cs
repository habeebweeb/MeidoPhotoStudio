namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class BackgroundPropModelSchema(short version = BackgroundPropModelSchema.SchemaVersion) : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public PropType Type =>
        PropType.Background;

    public short Version { get; } = version;

    public string ID { get; init; }
}
