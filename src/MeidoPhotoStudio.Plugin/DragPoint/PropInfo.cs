using System.IO;

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

    public static PropInfo FromModItem(ModItem modItem) =>
        new(PropType.Mod)
        {
            Filename = modItem.IsOfficialMod ? Path.GetFileName(modItem.MenuFile) : modItem.MenuFile,
            SubFilename = modItem.BaseMenuFile,
        };

    public static PropInfo FromMyRoom(MyRoomItem myRoomItem) =>
        new(PropType.MyRoom)
        {
            MyRoomID = myRoomItem.ID,
            Filename = myRoomItem.PrefabName,
        };

    public static PropInfo FromBg(string name) =>
        new(PropType.Bg) { Filename = name };

    public static PropInfo FromGameProp(string name) =>
        new(PropType.Odogu) { Filename = name };
}
