namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PhotoBgPropModelSchema : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public PhotoBgPropModelSchema(short version = SchemaVersion) =>
        Version = version;

    public PropType Type =>
        PropType.PhotoBg;

    public short Version { get; }

    public long ID { get; init; }
}
