namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class PropInfoSchema(short version = PropInfoSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public PropInfo.PropType Type { get; init; }

    public string Filename { get; init; }

    public string SubFilename { get; init; }

    public int MyRoomID { get; init; }

    public string IconFile { get; init; }
}
