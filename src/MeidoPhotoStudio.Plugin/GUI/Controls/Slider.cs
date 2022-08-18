using System;
using System.Globalization;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class Slider : BaseControl
{
    private bool hasLabel;
    private string label;
    private float value;
    private float left;
    private float right;
    private float defaultValue;
    private string textFieldValue;
    private bool hasTextField;

    public Slider(string label, float left, float right, float value = 0, float defaultValue = 0)
    {
        Label = label;
        this.left = left;
        this.right = right;
        this.value = Utility.Bound(value, left, right);
        textFieldValue = FormatValue(this.value);
        DefaultValue = defaultValue;
    }

    public Slider(string label, SliderProp prop)
        : this(label, prop.Left, prop.Right, prop.Initial, prop.Default)
    {
    }

    public Slider(SliderProp prop)
        : this(string.Empty, prop.Left, prop.Right, prop.Initial, prop.Default)
    {
    }

    public bool HasReset { get; set; }

    public string Label
    {
        get => label;
        set
        {
            label = value;
            hasLabel = !string.IsNullOrEmpty(label);
        }
    }

    public float Value
    {
        get => value;
        set
        {
            this.value = Utility.Bound(value, Left, Right);

            if (hasTextField)
                textFieldValue = FormatValue(value);

            OnControlEvent(EventArgs.Empty);
        }
    }

    public float Left
    {
        get => left;
        set
        {
            left = value;
            this.value = Utility.Bound(value, left, right);
        }
    }

    public float Right
    {
        get => right;
        set
        {
            right = value;
            this.value = Utility.Bound(value, left, right);
        }
    }

    public float DefaultValue
    {
        get => defaultValue;
        set => defaultValue = Utility.Bound(value, Left, Right);
    }

    public bool HasTextField
    {
        get => hasTextField;
        set
        {
            hasTextField = value;

            if (hasTextField)
                textFieldValue = FormatValue(Value);
        }
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
                tempText = GUILayout.TextField(textFieldValue, MpsGui.SliderTextBoxStyle, GUILayout.Width(60f));

            if (HasReset && GUILayout.Button("|", MpsGui.SliderResetButtonStyle, GUILayout.Width(15f)))
            {
                Value = DefaultValue;
                tempText = textFieldValue = FormatValue(Value);
            }

            GUILayout.EndHorizontal();
        }

        var sliderStyle = hasUpper ? MpsGui.SliderStyle : MpsGui.SliderStyleNoLabel;
        var tempValue =
            GUILayout.HorizontalSlider(Value, Left, Right, sliderStyle, MpsGui.SliderThumbStyle, layoutOptions);

        if (hasUpper)
            GUILayout.EndVertical();

        if (HasTextField)
        {
            if (tempValue != Value)
                tempText = textFieldValue = FormatValue(tempValue);

            if (tempText != textFieldValue)
            {
                textFieldValue = tempText;

                if (float.TryParse(tempText, out var newValue))
                    tempValue = newValue;
            }
        }

        if (tempValue != Value)
            Value = tempValue;
    }

    public void SetBounds(float left, float right)
    {
        this.left = left;
        this.right = right;
        value = Utility.Bound(value, left, right);
    }

    private static string FormatValue(float value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);
}
