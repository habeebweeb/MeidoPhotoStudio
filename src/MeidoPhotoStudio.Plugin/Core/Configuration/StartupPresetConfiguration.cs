using BepInEx.Configuration;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class StartupPresetConfiguration
{
    private const string Section = "Startup Preset";
    private const string LoadOptionsSection = $"{Section}.Load Options";

    private readonly ConfigFile configFile;

    public StartupPresetConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        Enabled = configFile.Bind(
            Section,
            "Enabled",
            true,
            "Apply a scene preset when starting MeidoPhotoStudio.");

        UseCustomPreset = configFile.Bind(
            Section,
            "Use Custom Startup Preset",
            false,
            "Apply a custom scene preset when starting MeidoPhotoStudio rather than the default preset.");
        this.configFile = configFile;
    }

    public ConfigEntry<bool> Enabled { get; }

    public ConfigEntry<bool> UseCustomPreset { get; }

    public ConfigEntry<bool> GetLoadOptionEntry(LoadOption loadOption)
    {
        _ = loadOption ?? throw new ArgumentNullException(nameof(loadOption));

        if (configFile.TryGetEntry(LoadOptionsSection, loadOption.Path, out ConfigEntry<bool> entry))
            return entry;
        else
            entry = configFile.Bind(LoadOptionsSection, loadOption.Path, true);

        return entry;
    }
}
