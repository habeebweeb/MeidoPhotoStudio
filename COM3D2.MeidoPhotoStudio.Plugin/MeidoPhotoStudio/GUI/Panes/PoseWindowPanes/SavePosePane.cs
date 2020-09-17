using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SavePosePane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Button savePoseButton;
        private readonly TextField poseNameTextField;
        private readonly ComboBox categoryComboBox;
        private string categoryHeader;
        private string nameHeader;

        public SavePosePane(MeidoManager meidoManager)
        {
            Constants.CustomPoseChange += (s, a) =>
            {
                categoryComboBox.SetDropdownItems(Constants.CustomPoseGroupList.ToArray());
            };

            this.meidoManager = meidoManager;

            categoryHeader = Translation.Get("posePane", "categoryHeader");
            nameHeader = Translation.Get("posePane", "nameHeader");

            savePoseButton = new Button(Translation.Get("posePane", "saveButton"));
            savePoseButton.ControlEvent += OnSavePose;

            categoryComboBox = new ComboBox(Constants.CustomPoseGroupList.ToArray());
            poseNameTextField = new TextField();
            poseNameTextField.ControlEvent += OnSavePose;
        }

        protected override void ReloadTranslation()
        {
            categoryHeader = Translation.Get("posePane", "categoryHeader");
            nameHeader = Translation.Get("posePane", "nameHeader");
            savePoseButton.Label = Translation.Get("posePane", "saveButton");
        }

        public override void Draw()
        {
            GUI.enabled = meidoManager.HasActiveMeido;

            MiscGUI.Header(categoryHeader);
            categoryComboBox.Draw(GUILayout.Width(160f));

            MiscGUI.Header(nameHeader);
            GUILayout.BeginHorizontal();
            poseNameTextField.Draw(GUILayout.Width(160f));
            savePoseButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void OnSavePose(object sender, EventArgs args)
        {
            byte[] anmBinary = meidoManager.ActiveMeido.SerializePose();
            Constants.AddPose(anmBinary, poseNameTextField.Value, categoryComboBox.Value);
            this.poseNameTextField.Value = String.Empty;
        }
    }
}
