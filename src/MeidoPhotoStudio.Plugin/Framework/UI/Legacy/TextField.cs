namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class TextField : BaseControl
{
    private static int textFieldID = 961;

    private readonly string controlName = $"textField{ID}";

    private GUIContent placeholderContent = GUIContent.none;
    private bool hasPlacholder = false;
    private string placeholder = string.Empty;

    public event EventHandler GainedFocus;

    public event EventHandler LostFocus;

    public event EventHandler ChangedValue;

    public static LazyStyle Style { get; } = new(13, () => new(GUI.skin.textField));

    public string Value { get; set; } = string.Empty;

    public string Placeholder
    {
        get => placeholder;
        set
        {
            if (string.Equals(placeholder, value, StringComparison.Ordinal))
                return;

            if (string.IsNullOrEmpty(value))
            {
                hasPlacholder = false;
                placeholder = string.Empty;
                placeholderContent = GUIContent.none;
            }
            else
            {
                hasPlacholder = true;
                placeholder = value;
                placeholderContent = new(placeholder);
            }
        }
    }

    public bool HasFocus { get; private set; }

    private static LazyStyle PlaceholderStyle { get; } = new(
        13,
        () => new(GUI.skin.label)
        {
            padding = new(5, 5, 0, 0),
            alignment = TextAnchor.MiddleLeft,
            normal =
            {
                textColor = new(1f, 1f, 1f, 0.6f),
            },
        });

    private static int ID =>
        ++textFieldID;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, PlaceholderStyle, layoutOptions);

    public void Draw(GUIStyle textFieldStyle, params GUILayoutOption[] layoutOptions) =>
        Draw(textFieldStyle, PlaceholderStyle, layoutOptions);

    public void Draw(GUIStyle textFieldStyle, GUIStyle placeholderStyle, params GUILayoutOption[] layoutOptions)
    {
        GUI.SetNextControlName(controlName);

        var value = GUILayout.TextField(Value, textFieldStyle, layoutOptions);

        if (hasPlacholder && value.Length is 0)
        {
            var textFieldRect = GUILayoutUtility.GetLastRect();

            GUI.Label(textFieldRect, placeholderContent, placeholderStyle);
        }

        if (!string.Equals(value, Value, StringComparison.Ordinal))
        {
            Value = value;

            ChangedValue?.Invoke(this, EventArgs.Empty);
        }

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
