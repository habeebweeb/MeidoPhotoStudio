using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public static class MenuFileUtility
    {
        private static byte[] fileBuffer;
        public const string noCategory = "noCategory";

        public static readonly string[] MenuCategories =
        {
            noCategory, "acchat", "headset", "wear", "skirt", "onepiece", "mizugi", "bra", "panz", "stkg", "shoes",
            "acckami", "megane", "acchead", "acchana", "accmimi", "glove", "acckubi", "acckubiwa", "acckamisub",
            "accnip", "accude", "accheso", "accashi", "accsenaka", "accshippo", "accxxx"
        };

        private static readonly HashSet<string> accMpn = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static event EventHandler MenuFilesReadyChange;
        public static bool MenuFilesReady { get; private set; }

        static MenuFileUtility()
        {
            accMpn.UnionWith(MenuCategories.Skip(1));
            GameMain.Instance.StartCoroutine(CheckMenuDataBaseJob());
        }

        private static IEnumerator CheckMenuDataBaseJob()
        {
            if (MenuFilesReady) yield break;
            while (!GameMain.Instance.MenuDataBase.JobFinished()) yield return null;
            MenuFilesReady = true;
            MenuFilesReadyChange?.Invoke(null, EventArgs.Empty);
        }

        private static ref byte[] GetFileBuffer(long size)
        {
            if (fileBuffer == null) fileBuffer = new byte[Math.Max(500000, size)];
            else if (fileBuffer.Length < size) fileBuffer = new byte[size];

            return ref fileBuffer;
        }

        public static byte[] ReadAFileBase(string filename)
        {
            using AFileBase aFileBase = GameUty.FileOpen(filename);

            if (aFileBase.IsValid() && aFileBase.GetSize() != 0)
            {
                ref byte[] buffer = ref GetFileBuffer(aFileBase.GetSize());

                aFileBase.Read(ref buffer, aFileBase.GetSize());

                return buffer;
            }

            Utility.LogError($"AFileBase '{filename}' is invalid");
            return null;
        }

        public static byte[] ReadOfficialMod(string filename)
        {
            using var fileStream = new FileStream(filename, FileMode.Open);

            if (fileStream.Length != 0)
            {
                ref byte[] buffer = ref GetFileBuffer(fileStream.Length);

                fileStream.Read(buffer, 0, (int) fileStream.Length);

                return buffer;
            }

            Utility.LogWarning($"Mod menu file '{filename}' is invalid");
            return null;
        }

        public static bool ParseNativeMenuFile(int menuIndex, ModItem modItem)
        {
            MenuDataBase menuDataBase = GameMain.Instance.MenuDataBase;
            menuDataBase.SetIndex(menuIndex);
            if (menuDataBase.GetBoDelOnly()) return false;
            modItem.Category = menuDataBase.GetCategoryMpnText();
            if (!accMpn.Contains(modItem.Category)) return false;
            modItem.MenuFile = menuDataBase.GetMenuFileName().ToLower();
            if (!ValidBG2MenuFile(modItem.MenuFile)) return false;
            modItem.Name = menuDataBase.GetMenuName();
            modItem.IconFile = menuDataBase.GetIconS();
            modItem.Priority = menuDataBase.GetPriority();
            return true;
        }

        public static void ParseMenuFile(string menuFile, ModItem modItem)
        {
            if (!ValidBG2MenuFile(menuFile)) return;

            byte[] buffer;

            try { buffer = ReadAFileBase(menuFile); }
            catch (Exception e)
            {
                Utility.LogError($"Could not read menu file '{menuFile}' because {e.Message}");
                return ;
            }

            try
            {
                using var binaryReader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8);

                if (binaryReader.ReadString() != "CM3D2_MENU") return;
                binaryReader.ReadInt32(); // file version
                binaryReader.ReadString(); // txt path
                modItem.Name = binaryReader.ReadString(); // name
                binaryReader.ReadString(); // category
                binaryReader.ReadString(); // description
                binaryReader.ReadInt32(); // idk (as long)

                while (true)
                {
                    int numberOfProps = binaryReader.ReadByte();
                    var menuPropString = string.Empty;

                    if (numberOfProps == 0) break;

                    for (var i = 0; i < numberOfProps; i++)
                    {
                        menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";
                    }

                    if (string.IsNullOrEmpty(menuPropString)) continue;

                    var header = UTY.GetStringCom(menuPropString);
                    string[] menuProps = UTY.GetStringList(menuPropString);

                    if (header == "end") break;

                    if (header == "category")
                    {
                        modItem.Category = menuProps[1];
                        if (!accMpn.Contains(modItem.Category)) return;
                    }
                    else if (header == "icons" || header == "icon")
                    {
                        modItem.IconFile = menuProps[1];
                        break;
                    }
                    else if (header == "priority") modItem.Priority = float.Parse(menuProps[1]);
                }
            }
            catch (Exception e)
            {
                Utility.LogWarning($"Could not parse menu file '{menuFile}' because {e.Message}");
            }
        }

        public static bool ParseModMenuFile(string modMenuFile, ModItem modItem)
        {
            if (!ValidBG2MenuFile(modMenuFile)) return false;

            byte[] modBuffer;

            try { modBuffer = ReadOfficialMod(modMenuFile); }
            catch (Exception e)
            {
                Utility.LogError($"Could not read mod menu file '{modMenuFile} because {e.Message}'");
                return false;
            }

            try
            {
                using var binaryReader = new BinaryReader(new MemoryStream(modBuffer), Encoding.UTF8);

                if (binaryReader.ReadString() != "CM3D2_MOD") return false;

                binaryReader.ReadInt32();
                var iconName = binaryReader.ReadString();
                var baseItemPath = binaryReader.ReadString().Replace(":", " ");
                modItem.BaseMenuFile = Path.GetFileName(baseItemPath);
                modItem.Name = binaryReader.ReadString();
                modItem.Category = binaryReader.ReadString();
                if (!accMpn.Contains(modItem.Category)) return false;
                binaryReader.ReadString();

                var mpnValue = binaryReader.ReadString();
                var mpn = MPN.null_mpn;
                try { mpn = (MPN) Enum.Parse(typeof(MPN), mpnValue, true); }
                catch { /* ignored */ }

                if (mpn != MPN.null_mpn) binaryReader.ReadString();

                binaryReader.ReadString();

                var entryCount = binaryReader.ReadInt32();
                for (var i = 0; i < entryCount; i++)
                {
                    var key = binaryReader.ReadString();
                    var count = binaryReader.ReadInt32();
                    byte[] data = binaryReader.ReadBytes(count);

                    if (!string.Equals(key, iconName, StringComparison.InvariantCultureIgnoreCase)) continue;

                    var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.LoadImage(data);
                    modItem.Icon = tex;
                    break;
                }
            }
            catch (Exception e)
            {
                Utility.LogWarning($"Could not parse mod menu file '{modMenuFile}' because {e}");
                return false;
            }

            return true;
        }

        public static bool ValidBG2MenuFile(ModItem modItem)
            => accMpn.Contains(modItem.Category) && ValidBG2MenuFile(modItem.MenuFile);

        public static bool ValidBG2MenuFile(string menu)
        {
            menu = Path.GetFileNameWithoutExtension(menu).ToLower();
            return !(menu.EndsWith("_del") || menu.Contains("zurashi") || menu.Contains("mekure")
                || menu.Contains("porori") || menu.Contains("moza") || menu.Contains("folder"));
        }
    }
}
