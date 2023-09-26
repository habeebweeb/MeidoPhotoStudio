using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin;

// TODO: Refactor configuration to not be a singleton.
public static class Configuration
{
    static Configuration()
    {
        var configPath = System.IO.Path.Combine(Constants.ConfigPath, $"{Plugin.PluginName}.cfg");

        Config = new(configPath, false);
    }

    public static ConfigFile Config { get; }
}
