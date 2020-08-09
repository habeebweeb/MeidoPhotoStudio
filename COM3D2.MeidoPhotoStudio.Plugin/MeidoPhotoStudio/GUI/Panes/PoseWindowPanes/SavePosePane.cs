using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SavePosePane : BasePane
    {
        private MeidoManager meidoManager;
        private Button savePoseButton;
        private TextField poseNameTextField;
        private ComboBox categoryComboBox;
        private string categoryHeader;
        private string nameHeader;

        public SavePosePane(MeidoManager meidoManager)
        {
            Constants.customPoseChange += (s, a) =>
            {
                this.categoryComboBox.SetDropdownItems(Constants.CustomPoseGroupList.ToArray());
            };

            this.meidoManager = meidoManager;

            this.categoryHeader = Translation.Get("posePane", "categoryHeader");
            this.nameHeader = Translation.Get("posePane", "nameHeader");

            this.savePoseButton = new Button(Translation.Get("posePane", "saveButton"));
            this.savePoseButton.ControlEvent += OnSavePose;

            this.categoryComboBox = new ComboBox(Constants.CustomPoseGroupList.ToArray());
            this.poseNameTextField = new TextField();
            this.poseNameTextField.ControlEvent += OnSavePose;
        }

        protected override void ReloadTranslation()
        {
            this.categoryHeader = Translation.Get("posePane", "categoryHeader");
            this.nameHeader = Translation.Get("posePane", "nameHeader");
            this.savePoseButton.Label = Translation.Get("posePane", "saveButton");
        }

        public override void Draw()
        {
            GUI.enabled = this.meidoManager.HasActiveMeido;

            MiscGUI.Header(categoryHeader);
            this.categoryComboBox.Draw(GUILayout.Width(160f));

            MiscGUI.Header(nameHeader);
            GUILayout.BeginHorizontal();
            this.poseNameTextField.Draw(GUILayout.Width(160f));
            this.savePoseButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void OnSavePose(object sender, EventArgs args)
        {
            byte[] anmBinary = this.meidoManager.ActiveMeido.SerializePose();
            Constants.AddPose(anmBinary, this.poseNameTextField.Value, this.categoryComboBox.Value);
            this.poseNameTextField.Value = String.Empty;
        }
    }
}
