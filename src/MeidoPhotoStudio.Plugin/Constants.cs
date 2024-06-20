namespace MeidoPhotoStudio.Plugin;

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
}
