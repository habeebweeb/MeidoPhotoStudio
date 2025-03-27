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

        PrecisePosingEnabled = this.configFile.Bind(
            Section,
            "Initial Precise Posing State",
            false,
            "Initial setting to use for precise posing after characters are called.");

        LimitJointsEnabled = this.configFile.Bind(
            Section,
            "Initial Limit Joints State",
            true,
            "Initial setting to use for joint limits after characters are called.");

        LimitDigitsEnabled = this.configFile.Bind(
            Section,
            "Initial Limit Digits State",
            true,
            "Initial setting to use for finger/toe limits after characters are called.");

        FreeLookEnabled = this.configFile.Bind(
            Section,
            "Initial Free Look State",
            true,
            "Initial setting to use for free-look after characters are called.");

        BlinkEnabled = this.configFile.Bind(
            Section,
            "Initial Blink State",
            true,
            "Initial setting to use for blink after characters are called.");

        PosingEnabled = this.configFile.Bind(
            Section,
            "Initial Posing State",
            true,
            "Initial setting to use for posing after characters are called.");
    }

    public ConfigEntry<bool> AutomaticallyApplyPlacement { get; }

    public ConfigEntry<PlacementService.Placement> PlacementPreset { get; }

    public ConfigEntry<bool> PrecisePosingEnabled { get; }

    public ConfigEntry<bool> LimitJointsEnabled { get; }

    public ConfigEntry<bool> LimitDigitsEnabled { get; }

    public ConfigEntry<bool> FreeLookEnabled { get; }

    public ConfigEntry<bool> BlinkEnabled { get; }

    public ConfigEntry<bool> PosingEnabled { get; }
}
