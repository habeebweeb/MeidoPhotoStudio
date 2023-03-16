using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public static class MenuFileUtility
{
    // TODO: What's this for?
    public const string NoCategory = "noCategory";

    public static readonly string[] MenuCategories =
    {
        NoCategory, "acchat", "headset", "wear", "skirt", "onepiece", "mizugi", "bra", "panz", "stkg", "shoes",
        "acckami", "megane", "acchead", "acchana", "accmimi", "glove", "acckubi", "acckubiwa", "acckamisub", "accnip",
        "accude", "accheso", "accashi", "accsenaka", "accshippo", "accxxx",
    };

    private static readonly HashSet<string> AccMpn = new(StringComparer.InvariantCultureIgnoreCase);

    private static byte[] fileBuffer;

    static MenuFileUtility()
    {
        AccMpn.UnionWith(MenuCategories.Skip(1));
        GameMain.Instance.StartCoroutine(CheckMenuDataBaseJob());
    }

    public static event EventHandler MenuFilesReadyChange;

    public static bool MenuFilesReady { get; private set; }

    public static byte[] ReadAFileBase(string filename)
    {
        using var aFileBase = GameUty.FileOpen(filename);

        if (!aFileBase.IsValid() || aFileBase.GetSize() is 0)
        {
            Utility.LogError($"AFileBase '{filename}' is invalid");

            return null;
        }

        // INFO: Don't really understand what this does.
        ref var buffer = ref GetFileBuffer(aFileBase.GetSize());

        aFileBase.Read(ref buffer, aFileBase.GetSize());

        return buffer;
    }

    public static byte[] ReadOfficialMod(string filename)
    {
        using var fileStream = new FileStream(filename, FileMode.Open);

        if (fileStream.Length is 0)
        {
            Utility.LogWarning($"Mod menu file '{filename}' is invalid");

            return null;
        }

        ref var buffer = ref GetFileBuffer(fileStream.Length);

        fileStream.Read(buffer, 0, (int)fileStream.Length);

        return buffer;
    }

    public static bool ParseNativeMenuFile(int menuIndex, ModItem modItem)
    {
        var menuDataBase = GameMain.Instance.MenuDataBase;

        menuDataBase.SetIndex(menuIndex);

        if (menuDataBase.GetBoDelOnly())
            return false;

        modItem.Category = menuDataBase.GetCategoryMpnText();

        if (!AccMpn.Contains(modItem.Category))
            return false;

        modItem.MenuFile = menuDataBase.GetMenuFileName().ToLower();

        modItem.Name = menuDataBase.GetMenuName();
        modItem.IconFile = menuDataBase.GetIconS();
        modItem.Priority = menuDataBase.GetPriority();

        return true;
    }

    public static void ParseMenuFile(string menuFile, ModItem modItem)
    {
        byte[] buffer;

        try
        {
            buffer = ReadAFileBase(menuFile);
        }
        catch (Exception e)
        {
            Utility.LogError($"Could not read menu file '{menuFile}' because {e.Message}");

            return;
        }

        try
        {
            using var binaryReader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8);

            if (binaryReader.ReadString() is not "CM3D2_MENU")
                return;

            binaryReader.ReadInt32(); // file version
            binaryReader.ReadString(); // txt path
            modItem.Name = binaryReader.ReadString(); // name
            binaryReader.ReadString(); // category
            binaryReader.ReadString(); // description
            binaryReader.ReadInt32(); // idk (as long)

            while (true)
            {
                var numberOfProps = binaryReader.ReadByte();
                var menuPropString = string.Empty;

                if (numberOfProps is 0)
                    break;

                for (var i = 0; i < numberOfProps; i++)
                    menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";

                if (string.IsNullOrEmpty(menuPropString))
                    continue;

                var header = UTY.GetStringCom(menuPropString);
                var menuProps = UTY.GetStringList(menuPropString);

                if (header is "end")
                    break;

                if (header is "category")
                {
                    modItem.Category = menuProps[1];

                    if (!AccMpn.Contains(modItem.Category))
                        return;
                }
                else if (header is "icons" or "icon")
                {
                    modItem.IconFile = menuProps[1];
                }
                else if (header is "priority")
                {
                    modItem.Priority = float.Parse(menuProps[1]);
                }
            }
        }
        catch (Exception e)
        {
            Utility.LogWarning($"Could not parse menu file '{menuFile}' because {e.Message}");
        }
    }

    public static bool ParseModMenuFile(string modMenuFile, ModItem modItem)
    {
        byte[] modBuffer;

        try
        {
            modBuffer = ReadOfficialMod(modMenuFile);
        }
        catch (Exception e)
        {
            Utility.LogError($"Could not read mod menu file '{modMenuFile} because {e.Message}'");

            return false;
        }

        try
        {
            using var binaryReader = new BinaryReader(new MemoryStream(modBuffer), Encoding.UTF8);

            if (binaryReader.ReadString() is not "CM3D2_MOD")
                return false;

            binaryReader.ReadInt32();

            var iconName = binaryReader.ReadString();
            var baseItemPath = binaryReader.ReadString().Replace(":", " ");

            modItem.BaseMenuFile = Path.GetFileName(baseItemPath);
            modItem.Name = binaryReader.ReadString();
            modItem.Category = binaryReader.ReadString();

            if (!AccMpn.Contains(modItem.Category))
                return false;

            binaryReader.ReadString();

            var mpnValue = binaryReader.ReadString();
            var mpn = MPN.null_mpn;

            try
            {
                mpn = (MPN)Enum.Parse(typeof(MPN), mpnValue, true);
            }
            catch
            {
                // Ignored.
            }

            if (mpn is not MPN.null_mpn)
                binaryReader.ReadString();

            binaryReader.ReadString();

            var entryCount = binaryReader.ReadInt32();

            for (var i = 0; i < entryCount; i++)
            {
                var key = binaryReader.ReadString();
                var count = binaryReader.ReadInt32();
                var data = binaryReader.ReadBytes(count);

                if (!string.Equals(key, iconName, StringComparison.InvariantCultureIgnoreCase))
                    continue;

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

    public static bool ValidBG2MenuFile(ModItem modItem) =>
        AccMpn.Contains(modItem.Category);

    private static IEnumerator CheckMenuDataBaseJob()
    {
        if (MenuFilesReady)
            yield break;

        while (!GameMain.Instance.MenuDataBase.JobFinished())
            yield return null;

        MenuFilesReady = true;
        MenuFilesReadyChange?.Invoke(null, EventArgs.Empty);
    }

    private static ref byte[] GetFileBuffer(long size)
    {
        if (fileBuffer is null)
            fileBuffer = new byte[Math.Max(500000, size)];
        else if (fileBuffer.Length < size)
            fileBuffer = new byte[size];

        return ref fileBuffer;
    }
}
