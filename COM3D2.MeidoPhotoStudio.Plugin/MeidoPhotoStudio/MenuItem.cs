using System.Globalization;
using System.IO;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class MenuItem
    {
        public string IconFile { get; set; }
        public Texture2D Icon { get; set; }
    }

    public class ModItem : MenuItem
    {
        public string MenuFile { get; set; }
        public string BaseMenuFile { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public float Priority { get; set; }
        public bool IsMod { get; private set; }
        public bool IsOfficialMod { get; private set; }

        public static ModItem OfficialMod(string menuFile) => new ModItem()
        {
            MenuFile = menuFile, IsMod = true, IsOfficialMod = true, Priority = 1000f
        };

        public static ModItem Mod(string menuFile) => new ModItem() { MenuFile = menuFile, IsMod = true };

        public ModItem() { }

        public ModItem(string menuFile) => MenuFile = menuFile;

        public override string ToString() => IsOfficialMod ? $"{Path.GetFileName(MenuFile)}#{BaseMenuFile}" : MenuFile;

        public static ModItem Deserialize(BinaryReader binaryReader) => new ModItem()
        {
            MenuFile = binaryReader.ReadNullableString(),
            BaseMenuFile = binaryReader.ReadNullableString(),
            IconFile = binaryReader.ReadNullableString(),
            Name = binaryReader.ReadNullableString(),
            Category = binaryReader.ReadNullableString(),
            Priority = float.Parse(binaryReader.ReadNullableString()),
            IsMod = binaryReader.ReadBoolean(),
            IsOfficialMod = binaryReader.ReadBoolean()
        };

        public void Serialize(BinaryWriter binaryWriter)
        {
            if (IsOfficialMod) return;

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

    public class MyRoomItem : MenuItem
    {
        public int ID { get; set; }
        public string PrefabName { get; set; }

        public override string ToString() => $"MYR_{ID}#{PrefabName}";
    }
}
