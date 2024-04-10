namespace MeidoPhotoStudio.Plugin;

public class PropInfo(PropInfo.PropType type)
{
    public enum PropType
    {
        Mod,
        MyRoom,
        Bg,
        Odogu,
    }

    public PropType Type { get; } = type;

    public string IconFile { get; set; }

    public string Filename { get; set; }

    public string SubFilename { get; set; }

    public int MyRoomID { get; set; }
}
