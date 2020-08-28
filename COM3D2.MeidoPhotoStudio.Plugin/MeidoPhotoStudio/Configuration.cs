using BepInEx.Configuration;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class Configuration
    {
        public static ConfigFile Config { get; private set; }

        static Configuration()
        {
            string configPath = System.IO.Path.Combine(Constants.configPath, $"{MeidoPhotoStudio.pluginName}.cfg");
            Config = new ConfigFile(configPath, false);
        }
    }
}
