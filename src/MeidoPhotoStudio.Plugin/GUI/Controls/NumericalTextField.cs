namespace MeidoPhotoStudio.Plugin;

public class NumericalTextField : BaseControl
{
    private float value;
    private string textFieldValue;

    public NumericalTextField(float initialValue)
    {
        value = initialValue;
        textFieldValue = FormatValue(value);
    }

    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(new(GUI.skin.textField), layoutOptions);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions)
    {
        var newText = GUILayout.TextField(textFieldValue, style, layoutOptions);

        if (string.Equals(newText, textFieldValue))
            return;

        textFieldValue = newText;

        if (!float.TryParse(textFieldValue, out var newValue))
            newValue = 0f;

        if (!Mathf.Approximately(Value, newValue))
            SetValue(newValue, updateTextField: false);
    }

    public void SetValueWithoutNotify(float value) =>
        SetValue(value, false);

    private static string FormatValue(float value) =>
        value.ToString("0.####");

    private void SetValue(float value, bool notify = true, bool updateTextField = true)
    {
        if (this.value == value)
            return;

        this.value = value;

        if (updateTextField)
            textFieldValue = FormatValue(this.value);

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }
}
