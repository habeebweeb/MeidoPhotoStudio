using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class TextArea : BaseControl
    {
        public string Value { get; set; } = string.Empty;
        public void Draw(GUIStyle textAreaStyle, params GUILayoutOption[] layoutOptions)
        {
            if (!Visible) return;
            Value = GUILayout.TextArea(Value, textAreaStyle, layoutOptions);
        }
        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            Draw(new GUIStyle(GUI.skin.textArea), layoutOptions);
        }
    }
}
