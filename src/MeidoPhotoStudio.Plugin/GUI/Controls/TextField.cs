using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class TextField : BaseControl
{
    private static int textFieldID = 961;

    private readonly string controlName = $"textField{ID}";

    public string Value { get; set; } = string.Empty;

    private static int ID =>
        ++textFieldID;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(new(GUI.skin.textField), layoutOptions);

    public void Draw(GUIStyle textFieldStyle, params GUILayoutOption[] layoutOptions)
    {
        GUI.SetNextControlName(controlName);
        Value = GUILayout.TextField(Value, textFieldStyle, layoutOptions);

        if (Event.current.isKey && Event.current.keyCode is KeyCode.Return)
            OnControlEvent(EventArgs.Empty);
    }
}
