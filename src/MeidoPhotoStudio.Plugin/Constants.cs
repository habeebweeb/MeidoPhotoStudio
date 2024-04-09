using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin;

// TODO: 😮 Holy moly! Destroy all static utility classes.
public static class Constants
{
    public const string SceneDirectory = "Scenes";
    public const string KankyoDirectory = "Environments";
    public const string ConfigDirectory = "MeidoPhotoStudio";
    public const string TranslationDirectory = "Translations";

    public static readonly string ScenesPath;
    public static readonly string KankyoPath;
    public static readonly string ConfigPath;

    public static readonly int DropdownWindowID = 777;

    public static readonly List<string> SceneDirectoryList = [];
    public static readonly List<string> KankyoDirectoryList = [];

    static Constants()
    {
        ConfigPath = Path.Combine(BepInEx.Paths.ConfigPath, ConfigDirectory);

        ScenesPath = Path.Combine(ConfigPath, SceneDirectory);
        KankyoPath = Path.Combine(ConfigPath, KankyoDirectory);

        var directories = new[] { ConfigPath, ScenesPath, KankyoPath };

        foreach (var directory in directories)
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
    }

    public enum Window
    {
        Call,
        Pose,
        Face,
        BG,
        BG2,
        Main,
        Message,
        Save,
        SaveModal,
        Settings,
    }

    public static void Initialize()
    {
        InitializeSceneDirectories();
        InitializeKankyoDirectories();
    }

    public static void InitializeSceneDirectories()
    {
        SceneDirectoryList.Clear();
        SceneDirectoryList.Add(SceneDirectory);

        foreach (var directory in Directory.GetDirectories(ScenesPath))
            SceneDirectoryList.Add(new DirectoryInfo(directory).Name);

        SceneDirectoryList.Sort((a, b) => KeepAtTop(a, b, SceneDirectory, new WindowsLogicalStringComparer()));
    }

    public static void InitializeKankyoDirectories()
    {
        KankyoDirectoryList.Clear();
        KankyoDirectoryList.Add(KankyoDirectory);

        foreach (var directory in Directory.GetDirectories(KankyoPath))
            KankyoDirectoryList.Add(new DirectoryInfo(directory).Name);

        KankyoDirectoryList.Sort((a, b) => KeepAtTop(a, b, KankyoDirectory, new WindowsLogicalStringComparer()));
    }

    private static int KeepAtTop(string a, string b, string topItem, IComparer<string> comparer)
    {
        if (a == b)
            return 0;

        if (a == topItem)
            return -1;

        if (b == topItem)
            return 1;

        return comparer.Compare(a, b);
    }
}
