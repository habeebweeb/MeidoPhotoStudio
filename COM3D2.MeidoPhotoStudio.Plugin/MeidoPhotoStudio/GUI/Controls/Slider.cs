using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class Slider : BaseControl
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
        private float left;
        public float Left
        {
            get => this.left;
            set
            {
                this.left = value;
                this.Value = this.value;
            }
        }
        private float right;
        public float Right
        {
            get => this.right;
            set
            {
                this.right = value;
                this.Value = this.value;
            }
        }

        public Slider(string label, float left, float right, float value = 0)
        {
            Label = label;
            Left = left;
            Right = right;
            this.value = Utility.Bound(value, left, right);
        }

        public Slider(float min, float max, float value = 0) : this(String.Empty, min, max, value) { }

        public Slider(string label, SliderProp prop) : this(label, prop.Left, prop.Right, prop.Initial) { }

        public Slider(SliderProp prop) : this(String.Empty, prop.Left, prop.Right, prop.Initial) { }

        public void SetBounds(float left, float right)
        {
            this.left = left;
            this.right = right;
            this.Value = this.Value;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            if (!Visible) return;
            GUIStyle sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            sliderStyle.margin.bottom = 0;
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
            float value = GUILayout.HorizontalSlider(
                Value, Left, Right, sliderStyle, GUI.skin.horizontalSliderThumb, layoutOptions
            );
            if (hasLabel) GUILayout.EndVertical();
            if (value != Value) Value = value;
        }
    }

    public struct SliderProp
    {
        public float Left { get; }
        public float Right { get; }
        public float Initial { get; }

        public SliderProp(float left, float right, float initial = 0f)
        {
            this.Left = left;
            this.Right = right;
            this.Initial = initial;
        }
    }
}
