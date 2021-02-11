using System;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class Button : BaseControl
    {
        public string Label { get; set; }
        public Button(string label) => Label = label;
        public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
        {
            if (GUILayout.Button(Label, buttonStyle, layoutOptions)) OnControlEvent(EventArgs.Empty);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            Draw(buttonStyle, layoutOptions);
        }
    }
}
