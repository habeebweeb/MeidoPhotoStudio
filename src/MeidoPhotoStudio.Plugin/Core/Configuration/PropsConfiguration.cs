using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class PropsConfiguration
{
    private readonly ConfigFile configFile;
    private readonly ConfigEntry<bool> menuPropsConfigEntry;

    public PropsConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        menuPropsConfigEntry = this.configFile.Bind("Prop", "ModItemsOnly", false);
    }

    public bool ModMenuPropsOnly =>
        menuPropsConfigEntry.Value;
}
