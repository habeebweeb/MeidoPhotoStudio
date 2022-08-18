using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin;

public static class Configuration
{
    static Configuration()
    {
        var configPath = System.IO.Path.Combine(Constants.ConfigPath, $"{MeidoPhotoStudio.PluginName}.cfg");

        Config = new(configPath, false);
    }

    public static ConfigFile Config { get; }
}
