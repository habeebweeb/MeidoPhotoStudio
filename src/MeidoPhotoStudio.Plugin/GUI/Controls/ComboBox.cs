using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class ComboBox : BaseControl
{
    private readonly TextField textField = new();

    public ComboBox(string[] itemList)
    {
        BaseDropDown = new("▾", itemList);
        BaseDropDown.SelectionChange += (_, _) =>
            textField.Value = BaseDropDown.SelectedItem;

        Value = itemList[0];
    }

    public Dropdown BaseDropDown { get; }

    public string Value
    {
        get => textField.Value;
        set => textField.Value = value;
    }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
        };

        Draw(buttonStyle, layoutOptions);
    }

    public void SetDropdownItems(string[] itemList)
    {
        var oldValue = Value;

        BaseDropDown.SetDropdownItems(itemList);
        Value = oldValue;
    }

    public void SetDropdownItem(int index, string newItem) =>
        BaseDropDown.SetDropdownItem(index, newItem);

    public void SetDropdownItem(string newItem) =>
        BaseDropDown.SetDropdownItem(newItem);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions)
    {
        GUILayout.BeginHorizontal();
        textField.Draw(new(GUI.skin.textField), layoutOptions);
        BaseDropDown.Draw(style, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
    }
}
