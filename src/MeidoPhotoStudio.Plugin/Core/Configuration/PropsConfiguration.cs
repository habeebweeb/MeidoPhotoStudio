using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class PropsConfiguration
{
    private const string Section = "Props";

    private readonly ConfigFile configFile;

    public PropsConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        IgnoreGameMenuFiles = this.configFile.Bind(
            Section,
            "Ignore Game Menu Files",
            false,
            "Only show menu files within the 'Mod' folder and ignore the base game menu files.");

        InitialKeepPositionOnAttachState = this.configFile.Bind(
            Section,
            "Initial Keep Position On Attach State",
            true,
            "The initial state for whether or not to keep the prop's position when attaching to a character.");
    }

    public ConfigEntry<bool> IgnoreGameMenuFiles { get; }

    public ConfigEntry<bool> InitialKeepPositionOnAttachState { get; }
}
