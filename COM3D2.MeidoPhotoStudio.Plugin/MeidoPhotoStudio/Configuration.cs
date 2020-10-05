using BepInEx.Configuration;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public static class Configuration
    {
        public static ConfigFile Config { get; }

        static Configuration()
        {
            string configPath = System.IO.Path.Combine(Constants.configPath, $"{MeidoPhotoStudio.pluginName}.cfg");
            Config = new ConfigFile(configPath, false);
        }
    }
}
