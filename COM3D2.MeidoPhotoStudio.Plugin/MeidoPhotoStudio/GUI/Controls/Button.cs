using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Button : BaseControl
    {
        public string Label { get; set; }
        public Button(string label)
        {
            this.Label = label;
        }
        public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
        {
            if (!Visible) return;
            bool clicked = false;
            clicked = GUILayout.Button(Label, buttonStyle, layoutOptions);
            if (clicked) OnControlEvent(EventArgs.Empty);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            Draw(buttonStyle, layoutOptions);
        }
    }
}
