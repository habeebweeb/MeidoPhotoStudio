namespace MeidoPhotoStudio.Plugin;

public class PropInfo
{
    public PropInfo(PropType type) =>
        Type = type;

    public enum PropType
    {
        Mod,
        MyRoom,
        Bg,
        Odogu,
    }

    public PropType Type { get; }

    public string IconFile { get; set; }

    public string Filename { get; set; }

    public string SubFilename { get; set; }

    public int MyRoomID { get; set; }
}
