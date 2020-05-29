using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Slider : BaseControl
    {
        private bool hasLabel;
        private string label;
        public string Label
        {
            get => label;
            set
            {
                label = value;
                hasLabel = !string.IsNullOrEmpty(label);
            }
        }
        private float value;
        public float Value
        {
            get => value;
            set
            {
                this.value = Utility.Bound(value, this.Left, this.Right);
                OnControlEvent(EventArgs.Empty);
            }
        }
        public float Left { get; set; }
        public float Right { get; set; }

        public Slider(string label, float left, float right, float value = 0)
        {
            Label = label;
            Left = left;
            Right = right;
            this.value = Utility.Bound(value, left, right);
        }
        public Slider(float min, float max, float value = 0) : this("", min, max, value) { }
        public override void Draw(params GUILayoutOption[] layoutOptions)
        {

            if (!Visible) return;
            GUIStyle sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            if (hasLabel)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
                GUIStyle sliderLabelStyle = new GUIStyle(GUI.skin.label);
                sliderLabelStyle.padding.bottom = -5;
                sliderLabelStyle.margin = new RectOffset(0, 0, 0, 0);
                sliderLabelStyle.alignment = TextAnchor.LowerLeft;
                sliderLabelStyle.fontSize = 13;
                GUILayout.Label(Label, sliderLabelStyle, GUILayout.ExpandWidth(false));
            }
            else sliderStyle.margin.top = 10;
            float value = GUILayout.HorizontalSlider(Value, Left, Right, sliderStyle, GUI.skin.horizontalSliderThumb, layoutOptions);
            if (hasLabel) GUILayout.EndVertical();
            if (value != Value) Value = value;
        }
    }
}
