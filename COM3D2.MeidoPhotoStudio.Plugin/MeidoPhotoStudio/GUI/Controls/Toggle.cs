using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class Toggle : BaseControl
    {
        private bool value;
        public bool Value
        {
            get => value;
            set
            {
                this.value = value;
                OnControlEvent(EventArgs.Empty);
            }
        }

        public string Label { get; set; }

        public Toggle(string label, bool state = false)
        {
            Label = label;
            value = state;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            Draw(new GUIStyle(GUI.skin.toggle), layoutOptions);
        }

        public void Draw(GUIStyle toggleStyle, params GUILayoutOption[] layoutOptions)
        {
            bool value = GUILayout.Toggle(Value, Label, toggleStyle, layoutOptions);
            if (value != Value) Value = value;
        }
    }
}
