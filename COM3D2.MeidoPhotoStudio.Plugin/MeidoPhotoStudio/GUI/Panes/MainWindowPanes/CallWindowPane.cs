using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class CallWindowPane : BaseMainWindowPane
    {
        private readonly MeidoManager meidoManager;
        private readonly MaidSelectorPane maidSelectorPane;
        private readonly Dropdown placementDropdown;
        private readonly Button placementOKButton;

        public CallWindowPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            placementDropdown = new Dropdown(
                Translation.GetArray("placementDropdown", MaidPlacementUtility.placementTypes)
            );

            placementOKButton = new Button(Translation.Get("maidCallWindow", "okButton"));
            placementOKButton.ControlEvent += (o, a) => this.meidoManager.PlaceMeidos(
                MaidPlacementUtility.placementTypes[placementDropdown.SelectedItemIndex]
            );

            maidSelectorPane = AddPane(new MaidSelectorPane(this.meidoManager));
        }

        protected override void ReloadTranslation()
        {
            placementDropdown.SetDropdownItems(
                Translation.GetArray("placementDropdown", MaidPlacementUtility.placementTypes)
            );
            placementOKButton.Label = Translation.Get("maidCallWindow", "okButton");
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
    }
}
