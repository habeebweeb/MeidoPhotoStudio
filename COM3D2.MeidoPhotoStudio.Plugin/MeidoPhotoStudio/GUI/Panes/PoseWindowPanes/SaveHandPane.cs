using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SaveHandPane : BasePane
    {
        private MeidoManager meidoManager;
        private ComboBox categoryComboBox;
        private TextField handNameTextField;
        private Button saveLeftHandButton;
        private Button saveRightHandButton;
        private string categoryHeader;
        private string nameHeader;

        public SaveHandPane(MeidoManager meidoManager)
        {
            Constants.customHandChange += (s, a) =>
            {
                this.categoryComboBox.SetDropdownItems(Constants.CustomHandGroupList.ToArray());
            };

            this.meidoManager = meidoManager;

            this.categoryHeader = Translation.Get("handPane", "categoryHeader");

            this.nameHeader = Translation.Get("handPane", "nameHeader");

            this.saveLeftHandButton = new Button(Translation.Get("handPane", "saveLeftButton"));
            this.saveLeftHandButton.ControlEvent += (s, a) => SaveHand(right: false);

            this.saveRightHandButton = new Button(Translation.Get("handPane", "saveRightButton"));
            this.saveRightHandButton.ControlEvent += (s, a) => SaveHand(right: true);

            this.categoryComboBox = new ComboBox(Constants.CustomHandGroupList.ToArray());

            this.handNameTextField = new TextField();
        }

        protected override void ReloadTranslation()
        {
            this.categoryHeader = Translation.Get("handPane", "categoryHeader");
            this.nameHeader = Translation.Get("handPane", "nameHeader");
            this.saveLeftHandButton.Label = Translation.Get("handPane", "saveLeftButton");
            this.saveRightHandButton.Label = Translation.Get("handPane", "saveRightButton");
        }

        public override void Draw()
        {
            GUI.enabled = this.meidoManager.HasActiveMeido;

            MiscGUI.Header(categoryHeader);
            this.categoryComboBox.Draw(GUILayout.Width(165f));

            MiscGUI.Header(nameHeader);
            this.handNameTextField.Draw(GUILayout.Width(165f));

            GUILayout.BeginHorizontal();
            this.saveRightHandButton.Draw();
            this.saveLeftHandButton.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void SaveHand(bool right)
        {
            byte[] handBinary = this.meidoManager.ActiveMeido.IKManager.SerializeHand(right);
            Constants.AddHand(handBinary, right, this.handNameTextField.Value, this.categoryComboBox.Value);
            this.handNameTextField.Value = string.Empty;
        }
    }
}
