using BepInEx.Configuration;
using MeidoPhotoStudio.Database.Props.Menu;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class MenuPropsConfiguration : IMenuPropsConfiguration
{
    private readonly ConfigFile configFile;
    private readonly ConfigEntry<bool> menuPropsConfigEntry;

    public MenuPropsConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new System.ArgumentNullException(nameof(configFile));

        menuPropsConfigEntry = this.configFile.Bind("Prop", "ModItemsOnly", false);
    }

    public bool ModMenuPropsOnly =>
        menuPropsConfigEntry.Value;
}
