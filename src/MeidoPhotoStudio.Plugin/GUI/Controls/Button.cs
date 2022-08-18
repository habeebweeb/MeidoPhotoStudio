using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class Button : BaseControl
{
    public Button(string label) =>
        Label = label;

    public string Label { get; set; }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        var buttonStyle = new GUIStyle(GUI.skin.button);

        Draw(buttonStyle, layoutOptions);
    }

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        if (GUILayout.Button(Label, buttonStyle, layoutOptions))
            OnControlEvent(EventArgs.Empty);
    }
}
