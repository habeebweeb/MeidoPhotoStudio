namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class DeskPropModelSchema(short version = DeskPropModelSchema.SchemaVersion) : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public PropType Type =>
        PropType.Background;

    public short Version { get; } = version;

    public int ID { get; init; }
}
