using MeidoPhotoStudio.Plugin.Framework.UI;

namespace MeidoPhotoStudio.Plugin;

public class TextArea : BaseControl
{
    private static int textAreaID = 765;

    private readonly string controlName = $"textArea{ID}";

    public event EventHandler GainedFocus;

    public event EventHandler LostFocus;

    public static LazyStyle Style { get; } = new(13, () => new(GUI.skin.textArea));

    public string Value { get; set; } = string.Empty;

    public bool HasFocus { get; private set; }

    private static int ID =>
        ++textAreaID;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle textAreaStyle, params GUILayoutOption[] layoutOptions)
    {
        GUI.SetNextControlName(controlName);

        Value = GUILayout.TextArea(Value, textAreaStyle, layoutOptions);

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
