using System.Globalization;
using System.IO;

using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class ModItem : MenuItem
{
    public ModItem()
    {
    }

    public ModItem(string menuFile) =>
        MenuFile = menuFile;

    public string MenuFile { get; set; }

    public string BaseMenuFile { get; set; }

    public string Name { get; set; }

    public string Category { get; set; }

    public float Priority { get; set; }

    public bool IsMod { get; private set; }

    public bool IsOfficialMod { get; private set; }

    public static ModItem OfficialMod(string menuFile) =>
        new()
        {
            MenuFile = menuFile,
            IsMod = true,
            IsOfficialMod = true,
            Priority = 1000f,
        };

    public static ModItem Mod(string menuFile) =>
        new()
        {
            MenuFile = menuFile,
            IsMod = true,
        };

    public static ModItem Deserialize(BinaryReader binaryReader) =>
        new()
        {
            MenuFile = binaryReader.ReadNullableString(),
            BaseMenuFile = binaryReader.ReadNullableString(),
            IconFile = binaryReader.ReadNullableString(),
            Name = binaryReader.ReadNullableString(),
            Category = binaryReader.ReadNullableString(),
            Priority = float.Parse(binaryReader.ReadNullableString()),
            IsMod = binaryReader.ReadBoolean(),
            IsOfficialMod = binaryReader.ReadBoolean(),
        };

    public override string ToString() =>
        IsOfficialMod ? $"{Path.GetFileName(MenuFile)}#{BaseMenuFile}" : MenuFile;

    public void Serialize(BinaryWriter binaryWriter)
    {
        if (IsOfficialMod)
            return;

        binaryWriter.WriteNullableString(MenuFile);
        binaryWriter.WriteNullableString(BaseMenuFile);
        binaryWriter.WriteNullableString(IconFile);
        binaryWriter.WriteNullableString(Name);
        binaryWriter.WriteNullableString(Category);
        binaryWriter.WriteNullableString(Priority.ToString(CultureInfo.InvariantCulture));
        binaryWriter.Write(IsMod);
        binaryWriter.Write(IsOfficialMod);
    }
}
