using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class CallWindowPane : BaseMainWindowPane
    {
        private MeidoManager meidoManager;
        private MaidSelectorPane maidSelectorPane;
        private Dropdown placementDropdown;
        private Button placementOKButton;

        public CallWindowPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            placementDropdown = new Dropdown(
                Translation.GetArray("placementDropdown", MaidPlacementUtility.placementTypes)
            );
            Controls.Add(placementDropdown);

            placementOKButton = new Button(Translation.Get("maidCallWindow", "okButton"));
            placementOKButton.ControlEvent += (o, a) =>
            {
                meidoManager.PlaceMeidos(MaidPlacementUtility.placementTypes[placementDropdown.SelectedItemIndex]);
            };
            Controls.Add(placementOKButton);

            maidSelectorPane = AddPane(new MaidSelectorPane(meidoManager));
        }

        protected override void ReloadTranslation()
        {
            placementDropdown.SetDropdownItems(
                Translation.GetArray("placementDropdown", MaidPlacementUtility.placementTypes)
            );
            placementOKButton.Label = Translation.Get("maidCallWindow", "okButton");
        }

        public override void UpdatePanes()
        {
            base.UpdatePanes();
        }

        public override void Draw()
        {
            this.tabsPane.Draw();
            GUILayout.BeginHorizontal();
            placementDropdown.Draw(GUILayout.Width(150));
            placementOKButton.Draw();
            GUILayout.EndHorizontal();

            maidSelectorPane.Draw();
        }
    }
}
