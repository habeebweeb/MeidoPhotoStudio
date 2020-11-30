using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Slider : BaseControl
    {
        private const string textFormat = "F4";
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
                this.value = Utility.Bound(value, Left, Right);
                if (hasTextField) textFieldValue = Value.ToString(textFormat);
                OnControlEvent(EventArgs.Empty);
            }
        }

        private float left;

        public float Left
        {
            get => left;
            set
            {
                left = value;
                this.value = Utility.Bound(value, left, right);
            }
        }

        private float right;

        public float Right
        {
            get => right;
            set
            {
                right = value;
                this.value = Utility.Bound(value, left, right);
            }
        }
        private float defaultValue;
        public float DefaultValue
        {
            get => defaultValue;
            set => defaultValue = Utility.Bound(value, Left, Right);
        }

        private string textFieldValue;
        private bool hasTextField;
        public bool HasTextField
        {
            get => hasTextField;
            set
            {
                hasTextField = value;
                if (hasTextField) textFieldValue = Value.ToString(textFormat);
            }
        }
        public bool HasReset { get; set; }

        public Slider(string label, float left, float right, float value = 0, float defaultValue = 0)
        {
            textFieldValue = value.ToString(textFormat);
            Label = label;
            this.left = left;
            this.right = right;
            this.value = Utility.Bound(value, left, right);
            DefaultValue = defaultValue;
        }

        public Slider(string label, SliderProp prop) : this(label, prop.Left, prop.Right, prop.Initial, prop.Default) { }

        public Slider(SliderProp prop) : this(string.Empty, prop.Left, prop.Right, prop.Initial, prop.Default) { }

        public void SetBounds(float left, float right)
        {
            this.left = left;
            this.right = right;
            value = Utility.Bound(value, left, right);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            var hasUpper = hasLabel || HasTextField || HasReset;

            var tempText = string.Empty;

            if (hasUpper)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
                GUILayout.BeginHorizontal();

                if (hasLabel)
                {
                    GUILayout.Label(Label, MpsGui.SliderLabelStyle, GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                }

                if (HasTextField)
                {
                    tempText = GUILayout.TextField(textFieldValue, MpsGui.SliderTextBoxStyle, GUILayout.Width(60f));
                }

                if (HasReset && GUILayout.Button("|", MpsGui.SliderResetButtonStyle, GUILayout.Width(15f)))
                {
                    Value = DefaultValue;
                    tempText = textFieldValue = Value.ToString(textFormat);
                }
                GUILayout.EndHorizontal();
            }

            GUIStyle sliderStyle = hasUpper ? MpsGui.SliderStyle : MpsGui.SliderStyleNoLabel;

            var tempValue = GUILayout.HorizontalSlider(
                Value, Left, Right, sliderStyle, MpsGui.SliderThumbStyle, layoutOptions
            );

            if (hasUpper) GUILayout.EndVertical();

            if (HasTextField)
            {
                if (tempValue != Value) tempText = textFieldValue = tempValue.ToString(textFormat);

                if (tempText != textFieldValue)
                {
                    textFieldValue = tempText;
                    if (float.TryParse(tempText, out float newValue)) tempValue = newValue;
                }
            }

            if (tempValue != Value) Value = tempValue;
        }
    }

    public readonly struct SliderProp
    {
        public float Left { get; }
        public float Right { get; }
        public float Initial { get; }
        public float Default { get; }

        public SliderProp(float left, float right, float initial = 0f, float @default = 0f)
        {
            Left = left;
            Right = right;
            Initial = Utility.Bound(initial, left, right);
            Default = Utility.Bound(@default, left, right);
        }
    }
}
