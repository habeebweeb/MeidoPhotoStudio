namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class BackgroundPropModelSchema : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public BackgroundPropModelSchema(short version = SchemaVersion) =>
        Version = version;

    public PropType Type =>
        PropType.Background;

    public short Version { get; }

    public string ID { get; init; }
}
