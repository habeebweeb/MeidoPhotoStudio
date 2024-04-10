namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PhotoBgPropModelSchema(short version = PhotoBgPropModelSchema.SchemaVersion) : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public PropType Type =>
        PropType.PhotoBg;

    public short Version { get; } = version;

    public long ID { get; init; }
}
