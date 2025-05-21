using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class DragHandleConfiguration
{
    private const string GeneralSection = "Drag Handles";
    private const string ColourSection = $"{GeneralSection}.Colours";

    private readonly ConfigFile configFile;

    public DragHandleConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        AutomaticSelection = this.configFile.Bind(GeneralSection, "Automatically Select On Interaction", false);
        AutomaticTabSelection = this.configFile.Bind(GeneralSection, "Automatically Change Tab On Interaction", false);
        SmallTransformCube = this.configFile.Bind(GeneralSection, "Small Transform Drag Handles", false);
        CharacterTransformCube = this.configFile.Bind(GeneralSection, "Character Transform Drag Handle", false);
        UpperLimbDragHandleColour = this.configFile.Bind(ColourSection, "Upper Limb Drag Handle Colour", new Color(0.9f, 0.17f, 0f, 0.7f));
        MiddleLimbDragHandleColour = this.configFile.Bind(ColourSection, "Middle Limb Drag Handle Colour", new Color(0.01f, 0.85f, 0.39f, 0.7f));
        LowerLimbDragHandleColour = this.configFile.Bind(ColourSection, "Lower Limb Drag Handle Colour", new Color(0.05f, 0f, 0.86f, 0.7f));
        SpineDragHandleColour = this.configFile.Bind(ColourSection, "Spine Drag Handle Colour", new Color(0.45f, 0f, 0.8f, 0.6f));
        RootDragHandleColour = this.configFile.Bind(ColourSection, "Root Drag Handle Colour", new Color(0f, 0.84f, 0.6f, 0.7f));
        BaseDigitJointColour = this.configFile.Bind(ColourSection, "Base Digit Joint Drag Handle Colour", new Color(0.9f, 0.1f, 0f, 0.3f));
        MiddleDigitJointColour = this.configFile.Bind(ColourSection, "Middle Digit Joint Drag Handle Colour", new Color(0f, 0.9f, 0.07f, 0.3f));
        TipDigitJointColour = this.configFile.Bind(ColourSection, "Tip Digit Joint Drag Handle Colour", new Color(0.19f, 0.05f, 0.96f, 0.3f));
        ClothingDragHandleColour = this.configFile.Bind(ColourSection, "Clothing Gravity Drag Handle Colour", new Color(0.86f, 0.75f, 0.3f, 0.7f));
        HairDragHandleColour = this.configFile.Bind(ColourSection, "Hair Gravity Drag Handle Colour", new Color(0f, 0.47f, 0.19f, 0.7f));
    }

    public ConfigEntry<bool> AutomaticSelection { get; }

    public ConfigEntry<bool> AutomaticTabSelection { get; }

    public ConfigEntry<bool> SmallTransformCube { get; }

    public ConfigEntry<bool> CharacterTransformCube { get; }

    public ConfigEntry<Color> UpperLimbDragHandleColour { get; }

    public ConfigEntry<Color> MiddleLimbDragHandleColour { get; }

    public ConfigEntry<Color> LowerLimbDragHandleColour { get; }

    public ConfigEntry<Color> SpineDragHandleColour { get; }

    public ConfigEntry<Color> RootDragHandleColour { get; }

    public ConfigEntry<Color> BaseDigitJointColour { get; }

    public ConfigEntry<Color> MiddleDigitJointColour { get; }

    public ConfigEntry<Color> TipDigitJointColour { get; }

    public ConfigEntry<Color> ClothingDragHandleColour { get; }

    public ConfigEntry<Color> HairDragHandleColour { get; }
}
