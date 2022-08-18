using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class SaveFacePane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly ComboBox categoryComboBox;
    private readonly TextField faceNameTextField;
    private readonly Button saveFaceButton;

    private string categoryHeader;
    private string nameHeader;

    public SaveFacePane(MeidoManager meidoManager)
    {
        Constants.CustomFaceChange += (_, _) =>
            categoryComboBox.SetDropdownItems(Constants.CustomFaceGroupList.ToArray());

        this.meidoManager = meidoManager;

        categoryHeader = Translation.Get("faceSave", "categoryHeader");
        nameHeader = Translation.Get("faceSave", "nameHeader");

        saveFaceButton = new(Translation.Get("faceSave", "saveButton"));
        saveFaceButton.ControlEvent += (_, _) =>
            SaveFace();

        categoryComboBox = new(Constants.CustomFaceGroupList.ToArray());

        faceNameTextField = new();
    }

    public override void Draw()
    {
        GUI.enabled = meidoManager.HasActiveMeido;

        MpsGui.Header(categoryHeader);
        categoryComboBox.Draw(GUILayout.Width(165f));

        MpsGui.Header(nameHeader);
        GUILayout.BeginHorizontal();
        faceNameTextField.Draw(GUILayout.Width(160f));
        saveFaceButton.Draw(GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        categoryHeader = Translation.Get("faceSave", "categoryHeader");
        nameHeader = Translation.Get("faceSave", "nameHeader");
        saveFaceButton.Label = Translation.Get("faceSave", "saveButton");
    }

    private void SaveFace()
    {
        if (!meidoManager.HasActiveMeido)
            return;

        var meido = meidoManager.ActiveMeido;

        Constants.AddFacePreset(meido.SerializeFace(), faceNameTextField.Value, categoryComboBox.Value);
        faceNameTextField.Value = string.Empty;
    }
}
