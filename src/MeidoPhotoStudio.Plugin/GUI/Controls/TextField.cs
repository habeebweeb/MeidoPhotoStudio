using MeidoPhotoStudio.Plugin.Framework.UI;

namespace MeidoPhotoStudio.Plugin;

public class TextField : BaseControl
{
    private static int textFieldID = 961;

    private readonly string controlName = $"textField{ID}";

    public event EventHandler GainedFocus;

    public event EventHandler LostFocus;

    public static LazyStyle Style { get; } = new(13, () => new(GUI.skin.textField));

    public string Value { get; set; } = string.Empty;

    public bool HasFocus { get; private set; }

    private static int ID =>
        ++textFieldID;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle textFieldStyle, params GUILayoutOption[] layoutOptions)
    {
        GUI.SetNextControlName(controlName);

        Value = GUILayout.TextField(Value, textFieldStyle, layoutOptions);

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

        if (Event.current is { isKey: true, keyCode: KeyCode.Return or KeyCode.KeypadEnter } && string.Equals(focusedControl, controlName))
            OnControlEvent(EventArgs.Empty);
    }
}
