using MeidoPhotoStudio.Plugin.Service;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class CallWindowPane : BaseMainWindowPane
{
    private readonly MeidoManager meidoManager;
    private readonly MaidSelectorPane maidSelectorPane;
    private readonly Dropdown placementDropdown;
    private readonly Button placementOKButton;

    public CallWindowPane(MeidoManager meidoManager, CustomMaidSceneService customMaidSceneService)
    {
        this.meidoManager = meidoManager;

        placementDropdown = new(Translation.GetArray("placementDropdown", MaidPlacementUtility.PlacementTypes));

        placementOKButton = new(Translation.Get("maidCallWindow", "okButton"));
        placementOKButton.ControlEvent += (_, _) =>
            this.meidoManager.PlaceMeidos(MaidPlacementUtility.PlacementTypes[placementDropdown.SelectedItemIndex]);

        maidSelectorPane = AddPane(new MaidSelectorPane(this.meidoManager, customMaidSceneService));
    }

    public override void Draw()
    {
        tabsPane.Draw();

        GUILayout.BeginHorizontal();
        placementDropdown.Draw(GUILayout.Width(150));
        placementOKButton.Draw();
        GUILayout.EndHorizontal();

        maidSelectorPane.Draw();
    }

    protected override void ReloadTranslation()
    {
        placementDropdown.SetDropdownItems(
            Translation.GetArray("placementDropdown", MaidPlacementUtility.PlacementTypes));

        placementOKButton.Label = Translation.Get("maidCallWindow", "okButton");
    }
}
