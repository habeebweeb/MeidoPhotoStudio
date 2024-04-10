namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class MyRoomPropModelSchema(short version = MyRoomPropModelSchema.SchemaVersion) : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public PropType Type =>
        PropType.MyRoom;

    public short Version { get; } = version;

    public int ID { get; init; }
}
