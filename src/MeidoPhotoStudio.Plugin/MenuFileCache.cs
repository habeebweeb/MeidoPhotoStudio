using System.Collections.Generic;
using System.IO;

/*
    All of this is pretty much stolen from COM3D2.CacheEditMenu. Thanks Mr. Horsington.
    https://git.coder.horse/ghorsington/COM3D2.CacheEditMenu
*/
namespace MeidoPhotoStudio.Plugin;

public class MenuFileCache
{
    public static readonly string CachePath = Path.Combine(Constants.ConfigPath, "cache.dat");

    private const int CacheVersion = 765;

    private readonly Dictionary<string, ModItem> modItems;

    private bool rebuild;

    public MenuFileCache()
    {
        modItems = new();

        if (File.Exists(CachePath))
            Deserialize();
    }

    public ModItem this[string menu]
    {
        get => modItems[menu];
        set
        {
            if (modItems.ContainsKey(menu))
                return;

            rebuild = true;
            modItems[menu] = value;
        }
    }

    public bool Has(string menuFileName) =>
        modItems.ContainsKey(menuFileName);

    public void Serialize()
    {
        if (!rebuild)
            return;

        using var binaryWriter = new BinaryWriter(File.OpenWrite(CachePath));

        binaryWriter.Write(CacheVersion);

        foreach (var item in modItems.Values)
            item.Serialize(binaryWriter);
    }

    private void Deserialize()
    {
        using var binaryReader = new BinaryReader(File.OpenRead(CachePath));

        if (binaryReader.ReadInt32() is not CacheVersion)
        {
            Utility.LogInfo("Cache version out of date. Rebuilding");

            return;
        }

        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
        {
            var item = ModItem.Deserialize(binaryReader);

            modItems[item.MenuFile] = item;
        }
    }
}
