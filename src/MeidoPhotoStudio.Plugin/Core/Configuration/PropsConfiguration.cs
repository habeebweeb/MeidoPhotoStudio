using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class PropsConfiguration
{
    private readonly ConfigFile configFile;

    public PropsConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        IgnoreGameMenuFiles = this.configFile.Bind(
            "Props",
            "Ignore Game Menu Files",
            false,
            "Only show menu files within the 'Mod' folder and ignore the base game menu files.");
    }

    public ConfigEntry<bool> IgnoreGameMenuFiles { get; }
}
