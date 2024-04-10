namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PropsSchema(short version = PropsSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public List<PropControllerSchema> Props { get; init; }

    public List<DragHandleSchema> DragHandleSettings { get; init; }

    public List<AttachPointSchema> PropAttachment { get; init; }
}
