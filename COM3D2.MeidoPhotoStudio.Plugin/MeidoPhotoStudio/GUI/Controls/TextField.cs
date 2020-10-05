using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class TextField : BaseControl
    {
        private static int textFieldID = 961;
        private static int ID => ++textFieldID;
        private readonly string controlName = $"textField{ID}";
        public string Value { get; set; } = string.Empty;
        public void Draw(GUIStyle textFieldStyle, params GUILayoutOption[] layoutOptions)
        {
            GUI.SetNextControlName(controlName);
            Value = GUILayout.TextField(Value, textFieldStyle, layoutOptions);
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return) OnControlEvent(EventArgs.Empty);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            Draw(new GUIStyle(GUI.skin.textField), layoutOptions);
        }
    }
}
