using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Toggle : BaseControl
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

        public Toggle(bool state = false) : this("", state) { }

        public Toggle(string label, bool state = false)
        {
            Label = label;
            this.value = state;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            if (!Visible) return;
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
            bool value = GUILayout.Toggle(Value, Label, toggleStyle);
            if (value != Value) Value = value;
        }
    }
}
