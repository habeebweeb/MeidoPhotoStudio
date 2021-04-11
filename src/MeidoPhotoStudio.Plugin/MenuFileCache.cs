using System.IO;
using System.Collections.Generic;

/*
    All of this is pretty much stolen from COM3D2.CacheEditMenu. Thanks Mr. Horsington.
    https://git.coder.horse/ghorsington/COM3D2.CacheEditMenu
*/
namespace MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;

    public class MenuFileCache
    {
        private const int cacheVersion = 765;
        public static readonly string cachePath = Path.Combine(Constants.configPath, "cache.dat");
        private readonly Dictionary<string, ModItem> modItems;
        private bool rebuild;
        public ModItem this[string menu]
        {
            get => modItems[menu];
            set
            {
                if (!modItems.ContainsKey(menu))
                {
                    rebuild = true;
                    modItems[menu] = value;
                }
            }
        }

        public MenuFileCache()
        {
            modItems = new Dictionary<string, ModItem>();
            if (File.Exists(cachePath)) Deserialize();
        }

        public bool Has(string menuFileName) => modItems.ContainsKey(menuFileName);

        private void Deserialize()
        {
            using BinaryReader binaryReader = new BinaryReader(File.OpenRead(cachePath));
            if (binaryReader.ReadInt32() != cacheVersion)
            {
                Utility.LogInfo("Cache version out of date. Rebuilding");
                return;
            }
            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                ModItem item = ModItem.Deserialize(binaryReader);
                modItems[item.MenuFile] = item;
            }
        }

        public void Serialize()
        {
            if (!rebuild) return;
            using BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(cachePath));
            binaryWriter.Write(cacheVersion);
            foreach (ModItem item in modItems.Values) item.Serialize(binaryWriter);
        }
    }
}