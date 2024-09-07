using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterPlacementPane : BasePane
{
    private readonly PlacementService characterPlacementController;
    private readonly Dropdown<PlacementService.Placement> placementDropdown;
    private readonly Button applyPlacementButton;

    public CharacterPlacementPane(PlacementService characterPlacementController)
    {
        this.characterPlacementController = characterPlacementController ?? throw new ArgumentNullException(nameof(characterPlacementController));

        placementDropdown = new(
            Enum.GetValues(typeof(PlacementService.Placement))
                .Cast<PlacementService.Placement>()
                .ToArray(),
            formatter: PlacementTypeFormatter);

        applyPlacementButton = new(Translation.Get("maidCallWindow", "okButton"));
        applyPlacementButton.ControlEvent += OnPlacementButtonPushed;

        static string PlacementTypeFormatter(PlacementService.Placement placement, int index) =>
            Translation.Get("placementDropdown", placement.ToLower());
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
        placementDropdown.Reformat();

        applyPlacementButton.Label = Translation.Get("maidCallWindow", "okButton");
    }

    private void OnPlacementButtonPushed(object sender, EventArgs e) =>
        characterPlacementController.ApplyPlacement(placementDropdown.SelectedItem);
}
