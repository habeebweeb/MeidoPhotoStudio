using BepInEx.Configuration;
using MeidoPhotoStudio.Plugin.Core.Character;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class CharacterConfiguration
{
    private const string Section = "Character";

    private readonly ConfigFile configFile;

    public CharacterConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        AutomaticallyApplyPlacement = this.configFile.Bind(
            Section,
            "Automatically Apply Placement",
            true,
            "Automatically apply placement preset to initial characters called after activation");

        PlacementPreset = this.configFile.Bind(
            Section,
            "Initial Placement Preset",
            PlacementService.Placement.Parabolic,
            "Placement preset to use for automatic placement application");
    }

    public ConfigEntry<bool> AutomaticallyApplyPlacement { get; }

    public ConfigEntry<PlacementService.Placement> PlacementPreset { get; }
}
