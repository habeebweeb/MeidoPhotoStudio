namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class MyRoomPropModelSchema : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public MyRoomPropModelSchema(short version = SchemaVersion) =>
        Version = version;

    public PropType Type =>
        PropType.MyRoom;

    public short Version { get; }

    public int ID { get; init; }
}
