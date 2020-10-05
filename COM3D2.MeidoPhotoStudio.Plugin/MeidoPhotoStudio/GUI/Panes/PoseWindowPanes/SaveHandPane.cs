using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class SaveHandPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly ComboBox categoryComboBox;
        private readonly TextField handNameTextField;
        private readonly Button saveLeftHandButton;
        private readonly Button saveRightHandButton;
        private string categoryHeader;
        private string nameHeader;

        public SaveHandPane(MeidoManager meidoManager)
        {
            Constants.CustomHandChange += (s, a)
                => categoryComboBox.SetDropdownItems(Constants.CustomHandGroupList.ToArray());

            this.meidoManager = meidoManager;

            categoryHeader = Translation.Get("handPane", "categoryHeader");

            nameHeader = Translation.Get("handPane", "nameHeader");

            saveLeftHandButton = new Button(Translation.Get("handPane", "saveLeftButton"));
            saveLeftHandButton.ControlEvent += (s, a) => SaveHand(right: false);

            saveRightHandButton = new Button(Translation.Get("handPane", "saveRightButton"));
            saveRightHandButton.ControlEvent += (s, a) => SaveHand(right: true);

            categoryComboBox = new ComboBox(Constants.CustomHandGroupList.ToArray());

            handNameTextField = new TextField();
        }

        protected override void ReloadTranslation()
        {
            categoryHeader = Translation.Get("handPane", "categoryHeader");
            nameHeader = Translation.Get("handPane", "nameHeader");
            saveLeftHandButton.Label = Translation.Get("handPane", "saveLeftButton");
            saveRightHandButton.Label = Translation.Get("handPane", "saveRightButton");
        }

        public override void Draw()
        {
            GUI.enabled = meidoManager.HasActiveMeido;

            MpsGui.Header(categoryHeader);
            categoryComboBox.Draw(GUILayout.Width(165f));

            MpsGui.Header(nameHeader);
            handNameTextField.Draw(GUILayout.Width(165f));

            GUILayout.BeginHorizontal();
            saveRightHandButton.Draw();
            saveLeftHandButton.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void SaveHand(bool right)
        {
            byte[] handBinary = meidoManager.ActiveMeido.IKManager.SerializeHand(right);
            Constants.AddHand(handBinary, right, handNameTextField.Value, categoryComboBox.Value);
            handNameTextField.Value = string.Empty;
        }
    }
}
