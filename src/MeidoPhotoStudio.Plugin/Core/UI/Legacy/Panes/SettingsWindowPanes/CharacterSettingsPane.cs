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
    private readonly Header initialSettingHeader;
    private readonly Label settingExplanationLabel;
    private readonly Toggle precisePosingToggle;
    private readonly Toggle limitJointsToggle;
    private readonly Toggle limitDigitsToggle;
    private readonly Toggle freeLookToggle;
    private readonly Toggle blinkToggle;

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

        initialSettingHeader = new(new LocalizableGUIContent(translation, "characterSettingsPane", "initialSettingsHeader"));
        settingExplanationLabel = new(new LocalizableGUIContent(translation, "characterSettingsPane", "initialSettingsExplanation"));

        precisePosingToggle = new(
            new LocalizableGUIContent(translation, "characterSettingsPane", "precisePosingToggle"),
            this.characterConfiguration.PrecisePosingEnabled.Value);

        precisePosingToggle.ControlEvent += OnPrecisePosingToggleChanged;

        limitJointsToggle = new(
            new LocalizableGUIContent(translation, "characterSettingsPane", "limitJointsToggle"),
            this.characterConfiguration.LimitJointsEnabled.Value);

        limitJointsToggle.ControlEvent += OnLimitJointsToggleChanged;

        limitDigitsToggle = new(
            new LocalizableGUIContent(translation, "characterSettingsPane", "limitDigitsToggle"),
            this.characterConfiguration.LimitDigitsEnabled.Value);

        limitDigitsToggle.ControlEvent += OnLimitDigitsToggleChanged;

        freeLookToggle = new(
            new LocalizableGUIContent(translation, "characterSettingsPane", "freeLookToggle"),
            this.characterConfiguration.FreeLookEnabled.Value);

        freeLookToggle.ControlEvent += OnFreeLookToggleChanged;

        blinkToggle = new(
            new LocalizableGUIContent(translation, "characterSettingsPane", "blinkToggle"),
            this.characterConfiguration.BlinkEnabled.Value);

        blinkToggle.ControlEvent += OnBlinkToggleChanged;

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

        initialSettingHeader.Draw();
        settingExplanationLabel.Draw();

        precisePosingToggle.Draw();
        limitJointsToggle.Draw();
        limitDigitsToggle.Draw();
        freeLookToggle.Draw();
        blinkToggle.Draw();
    }

    private void OnPrecisePosingToggleChanged(object sender, EventArgs e) =>
        characterConfiguration.PrecisePosingEnabled.Value = precisePosingToggle.Value;

    private void OnLimitJointsToggleChanged(object sender, EventArgs e) =>
        characterConfiguration.LimitJointsEnabled.Value = limitJointsToggle.Value;

    private void OnLimitDigitsToggleChanged(object sender, EventArgs e) =>
        characterConfiguration.LimitDigitsEnabled.Value = limitDigitsToggle.Value;

    private void OnFreeLookToggleChanged(object sender, EventArgs e) =>
        characterConfiguration.FreeLookEnabled.Value = freeLookToggle.Value;

    private void OnBlinkToggleChanged(object sender, EventArgs e) =>
        characterConfiguration.BlinkEnabled.Value = blinkToggle.Value;

    private void OnAutomaticPlacementToggleChanged(object sender, EventArgs e) =>
        automaticCharacterPlacementController.Enabled = characterConfiguration.AutomaticallyApplyPlacement.Value
            = automaticPlacementToggle.Value;

    private void OnPlacementSelectionChanged(object sender, DropdownEventArgs<PlacementService.Placement> e) =>
        automaticCharacterPlacementController.PlacementType = characterConfiguration.PlacementPreset.Value = e.Item;
}
