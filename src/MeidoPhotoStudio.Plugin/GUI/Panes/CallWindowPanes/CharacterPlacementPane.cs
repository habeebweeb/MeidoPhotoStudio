using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class CharacterPlacementPane : BasePane
{
    private readonly PlacementService characterPlacementController;
    private readonly Dropdown placementDropdown;
    private readonly Button applyPlacementButton;
    private readonly PlacementService.Placement[] placementTypes;

    public CharacterPlacementPane(PlacementService characterPlacementController)
    {
        this.characterPlacementController = characterPlacementController ?? throw new ArgumentNullException(nameof(characterPlacementController));

        placementTypes = Enum.GetValues(typeof(PlacementService.Placement))
            .Cast<PlacementService.Placement>()
            .ToArray();

        placementDropdown = new(placementTypes
            .Select(placement => placement.ToLower())
            .Select(placement => Translation.Get("placementDropdown", placement))
            .ToArray());

        applyPlacementButton = new(Translation.Get("maidCallWindow", "okButton"));
        applyPlacementButton.ControlEvent += OnPlacementButtonPushed;
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();
        placementDropdown.Draw(GUILayout.Width(150));
        applyPlacementButton.Draw();
        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        placementDropdown.SetDropdownItemsWithoutNotify(placementTypes
            .Select(placement => placement.ToLower())
            .Select(placement => Translation.Get("placementDropdown", placement))
            .ToArray());

        applyPlacementButton.Label = Translation.Get("maidCallWindow", "okButton");
    }

    private void OnPlacementButtonPushed(object sender, EventArgs e) =>
        characterPlacementController.ApplyPlacement(placementTypes[placementDropdown.SelectedItemIndex]);
}
