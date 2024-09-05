using MeidoPhotoStudio.Plugin.Framework.UI;

namespace MeidoPhotoStudio.Plugin;

public class NumericalTextField : BaseControl
{
    private static int textFieldID = 961;

    private readonly string controlName = $"numericalTextField{ID}";

    private float value;
    private string textFieldValue;

    public NumericalTextField(float initialValue)
    {
        value = initialValue;
        textFieldValue = FormatValue(value);
    }

    public event EventHandler GainedFocus;

    public event EventHandler LostFocus;

    public static LazyStyle Style { get; } = new(13, () => new(GUI.skin.textField));

    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public bool HasFocus { get; private set; }

    private static int ID =>
        ++textFieldID;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions)
    {
        GUI.SetNextControlName(controlName);

        var newText = GUILayout.TextField(textFieldValue, style, layoutOptions);
        var focusedControl = GUI.GetNameOfFocusedControl();

        if (!HasFocus && string.Equals(focusedControl, controlName, StringComparison.Ordinal))
        {
            HasFocus = true;

            GainedFocus?.Invoke(this, EventArgs.Empty);
        }
        else if (HasFocus && !string.Equals(focusedControl, controlName, StringComparison.Ordinal))
        {
            HasFocus = false;

            LostFocus?.Invoke(this, EventArgs.Empty);
        }

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

        if (!HasFocus && updateTextField)
            textFieldValue = FormatValue(this.value);

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }
}
