namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PropsSchema
{
    public const short SchemaVersion = 2;

    public PropsSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public List<PropControllerSchema> Props { get; init; }

    public List<DragHandleSchema> DragHandleSettings { get; init; }

    public List<AttachPointSchema> PropAttachment { get; init; }
}
