using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

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
        Constants.CustomHandChange += (_, _) =>
            categoryComboBox.SetDropdownItems(Constants.CustomHandGroupList.ToArray());

        this.meidoManager = meidoManager;

        categoryHeader = Translation.Get("handPane", "categoryHeader");

        nameHeader = Translation.Get("handPane", "nameHeader");

        saveLeftHandButton = new(Translation.Get("handPane", "saveLeftButton"));
        saveLeftHandButton.ControlEvent += (_, _) =>
            SaveHand(right: false);

        saveRightHandButton = new(Translation.Get("handPane", "saveRightButton"));
        saveRightHandButton.ControlEvent += (_, _) =>
            SaveHand(right: true);

        categoryComboBox = new(Constants.CustomHandGroupList.ToArray());

        handNameTextField = new();
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

    protected override void ReloadTranslation()
    {
        categoryHeader = Translation.Get("handPane", "categoryHeader");
        nameHeader = Translation.Get("handPane", "nameHeader");
        saveLeftHandButton.Label = Translation.Get("handPane", "saveLeftButton");
        saveRightHandButton.Label = Translation.Get("handPane", "saveRightButton");
    }

    private void SaveHand(bool right)
    {
        var handBinary = meidoManager.ActiveMeido.IKManager.SerializeHand(right);

        Constants.AddHand(handBinary, right, handNameTextField.Value, categoryComboBox.Value);
        handNameTextField.Value = string.Empty;
    }
}
