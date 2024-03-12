namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PropInfoSchema
{
    public const short SchemaVersion = 1;

    public PropInfoSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public PropInfo.PropType Type { get; init; }

    public string Filename { get; init; }

    public string SubFilename { get; init; }

    public int MyRoomID { get; init; }

    public string IconFile { get; init; }
}
