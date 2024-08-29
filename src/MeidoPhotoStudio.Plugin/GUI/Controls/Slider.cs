namespace MeidoPhotoStudio.Plugin;

public class Slider : BaseControl
{
    private bool hasLabel;
    private string label;
    private float value;
    private float left;
    private float right;
    private float defaultValue;
    private bool hasTextField;
    private float temporaryValue;
    private NumericalTextField textField;

    public Slider(string label, float left, float right, float value = 0, float defaultValue = 0)
    {
        Label = label;
        this.left = left;
        this.right = right;
        SetValue(value, false);
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

    public event EventHandler StartedInteraction;

    public event EventHandler EndedInteraction;

    public event EventHandler PushingResetButton;

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
        set => SetValue(value);
    }

    public float Left
    {
        get => left;
        set
        {
            left = value;
            Value = this.value;
        }
    }

    public float Right
    {
        get => right;
        set
        {
            right = value;
            Value = this.value;
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
            {
                textField = new(Value);
                textField.ControlEvent += TextFieldInputChangedHandler;
            }
            else
            {
                if (textField is not null)
                    textField.ControlEvent -= TextFieldInputChangedHandler;

                textField = null;
            }
        }
    }

    public bool Dragging { get; private set; }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        var hasUpper = hasLabel || HasTextField || HasReset;

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
                textField.Draw(MpsGui.SliderTextBoxStyle, GUILayout.Width(60f));

            if (HasReset && GUILayout.Button("|", MpsGui.SliderResetButtonStyle, GUILayout.Width(15f)))
                OnResetButtonPushed();

            GUILayout.EndHorizontal();
        }

        var sliderStyle = hasUpper ? MpsGui.SliderStyle : MpsGui.SliderStyleNoLabel;

        temporaryValue =
            GUILayout.HorizontalSlider(temporaryValue, Left, Right, sliderStyle, MpsGui.SliderThumbStyle, layoutOptions);

        var @event = Event.current;

        if (!Dragging
            && UnityEngine.Input.GetMouseButtonDown(0)
            && @event.type is EventType.Repaint
            && GUILayoutUtility.GetLastRect().Contains(@event.mousePosition))
        {
            Dragging = true;

            StartedInteraction?.Invoke(this, EventArgs.Empty);
        }

        if (hasUpper)
            GUILayout.EndVertical();

        if (@event.type is EventType.Repaint && !Mathf.Approximately(Value, temporaryValue))
            Value = temporaryValue;

        if (Dragging && UnityEngine.Input.GetMouseButtonUp(0))
        {
            Dragging = false;

            EndedInteraction?.Invoke(this, EventArgs.Empty);
        }

        void OnResetButtonPushed()
        {
            PushingResetButton?.Invoke(this, EventArgs.Empty);

            ResetValue();
        }
    }

    public void SetValueWithoutNotify(float value) =>
        SetValue(value, false);

    public void ResetValue() =>
        Value = DefaultValue;

    private void SetValue(float value, bool notify = true)
    {
        var newValue = Utility.Bound(value, Left, Right);

        if (this.value == newValue)
            return;

        this.value = temporaryValue = newValue;

        textField?.SetValueWithoutNotify(this.value);

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }

    private void TextFieldInputChangedHandler(object sender, EventArgs e) =>
        SetValue(textField.Value);
}
