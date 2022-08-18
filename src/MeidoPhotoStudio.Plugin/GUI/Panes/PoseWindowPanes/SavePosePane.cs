using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class SavePosePane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Button savePoseButton;
    private readonly TextField poseNameTextField;
    private readonly ComboBox categoryComboBox;

    private string categoryHeader;
    private string nameHeader;

    public SavePosePane(MeidoManager meidoManager)
    {
        Constants.CustomPoseChange += (_, _) =>
            categoryComboBox.SetDropdownItems(Constants.CustomPoseGroupList.ToArray());

        this.meidoManager = meidoManager;

        categoryHeader = Translation.Get("posePane", "categoryHeader");
        nameHeader = Translation.Get("posePane", "nameHeader");

        savePoseButton = new(Translation.Get("posePane", "saveButton"));
        savePoseButton.ControlEvent += OnSavePose;

        categoryComboBox = new(Constants.CustomPoseGroupList.ToArray());

        poseNameTextField = new();
        poseNameTextField.ControlEvent += OnSavePose;
    }

    public override void Draw()
    {
        GUI.enabled = meidoManager.HasActiveMeido;

        MpsGui.Header(categoryHeader);
        categoryComboBox.Draw(GUILayout.Width(160f));

        MpsGui.Header(nameHeader);
        GUILayout.BeginHorizontal();
        poseNameTextField.Draw(GUILayout.Width(160f));
        savePoseButton.Draw(GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        categoryHeader = Translation.Get("posePane", "categoryHeader");
        nameHeader = Translation.Get("posePane", "nameHeader");
        savePoseButton.Label = Translation.Get("posePane", "saveButton");
    }

    private void OnSavePose(object sender, EventArgs args)
    {
        var anmBinary = meidoManager.ActiveMeido.SerializePose();

        Constants.AddPose(anmBinary, poseNameTextField.Value, categoryComboBox.Value);
        poseNameTextField.Value = string.Empty;
    }
}
