namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class DeskPropModelSchema : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public DeskPropModelSchema(short version = SchemaVersion) =>
        Version = version;

    public PropType Type =>
        PropType.Background;

    public short Version { get; }

    public int ID { get; init; }
}
