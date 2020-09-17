using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class Button : BaseControl
    {
        public string Label { get; set; }
        public Button(string label)
        {
            Label = label;
        }
        public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
        {
            if (!Visible) return;
            if (GUILayout.Button(Label, buttonStyle, layoutOptions)) OnControlEvent(EventArgs.Empty);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            Draw(buttonStyle, layoutOptions);
        }
    }
}
