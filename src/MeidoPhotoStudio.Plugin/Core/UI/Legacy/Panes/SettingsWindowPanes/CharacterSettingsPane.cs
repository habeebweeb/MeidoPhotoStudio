using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterSettingsPane : BasePane
{
    private readonly CharacterConfiguration characterConfiguration;
    private readonly AutomaticCharacterPlacementController automaticCharacterPlacementController;
    private readonly Header automaticPlacementSettingHeader;
    private readonly Label placementSettingExplanationLabel;
    private readonly Toggle automaticPlacementToggle;
    private readonly Dropdown<PlacementService.Placement> placementsPresetsDropdown;

    public CharacterSettingsPane(
        Translation translation,
        CharacterConfiguration characterConfiguration,
        AutomaticCharacterPlacementController automaticCharacterPlacementController)
    {
        this.characterConfiguration = characterConfiguration
            ?? throw new ArgumentNullException(nameof(characterConfiguration));

        this.automaticCharacterPlacementController = automaticCharacterPlacementController
            ?? throw new ArgumentNullException(nameof(automaticCharacterPlacementController));

        automaticPlacementSettingHeader = new(new LocalizableGUIContent(translation, "characterSettingsPane", "automaticPlacementHeader"));

        placementSettingExplanationLabel = new(new LocalizableGUIContent(translation, "characterSettingsPane", "automaticPlacementExplanation"));

        automaticPlacementToggle = new(
            new LocalizableGUIContent(translation, "characterSettingsPane", "automaticPlacementEnabledToggle"),
            this.characterConfiguration.AutomaticallyApplyPlacement.Value);

        automaticPlacementToggle.ControlEvent += OnAutomaticPlacementToggleChanged;

        var placementTypes = Enum.GetValues(typeof(PlacementService.Placement))
            .Cast<PlacementService.Placement>()
            .ToArray();

        var currentPlacementTypeIndex = Array.IndexOf(placementTypes, characterConfiguration.PlacementPreset.Value);

        if (currentPlacementTypeIndex is -1)
            currentPlacementTypeIndex = Array.IndexOf(placementTypes, PlacementService.Placement.Parabolic);

        placementsPresetsDropdown = new(
            placementTypes,
            currentPlacementTypeIndex,
            formatter: PlacementTypeFormatter);

        placementsPresetsDropdown.SelectionChanged += OnPlacementSelectionChanged;

        LabelledDropdownItem PlacementTypeFormatter(PlacementService.Placement placement, int index) =>
            new(translation["placementDropdown", placement.ToLower()]);
    }

    public override void Draw()
    {
        UIUtility.DrawBlackLine();

        automaticPlacementSettingHeader.Draw();

        placementSettingExplanationLabel.Draw();

        GUILayout.BeginHorizontal();

        automaticPlacementToggle.Draw();

        GUI.enabled = Parent.Enabled && characterConfiguration.AutomaticallyApplyPlacement.Value;

        placementsPresetsDropdown.Draw(GUILayout.Width(UIUtility.Scaled(200)));

        GUI.enabled = Parent.Enabled;

        GUILayout.EndHorizontal();
    }

    private void OnAutomaticPlacementToggleChanged(object sender, EventArgs e) =>
        automaticCharacterPlacementController.Enabled = characterConfiguration.AutomaticallyApplyPlacement.Value
            = automaticPlacementToggle.Value;

    private void OnPlacementSelectionChanged(object sender, DropdownEventArgs<PlacementService.Placement> e) =>
        automaticCharacterPlacementController.PlacementType = characterConfiguration.PlacementPreset.Value = e.Item;
}
