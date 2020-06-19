using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class TextField : BaseControl
    {
        public string Value { get; set; } = string.Empty;
        public void Draw(GUIStyle textFieldStyle, params GUILayoutOption[] layoutOptions)
        {
            if (!Visible) return;
            Value = GUILayout.TextField(Value, textFieldStyle, layoutOptions);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            Draw(new GUIStyle(GUI.skin.textField), layoutOptions);
        }
    }
}
