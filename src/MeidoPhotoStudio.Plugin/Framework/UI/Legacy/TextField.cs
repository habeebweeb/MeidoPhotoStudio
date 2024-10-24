namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class TextField : BaseControl
{
    private const string EncodedClearButtonImage =
        """
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAABY0lEQVQ4y53TvWpUARAF4G+vq4V/
        lSA4pQ8gglgJLqgvYLNqoxYqioWQap8gld0GiaURyaKdjQqBTW0jsbFJOXkHZf0pnCvXy0bFUw7n
        DDNn5gz0kJkXMcYIp6u8izlmEbHd5Q86wlOY4KE/Y4rViNj71aDEj3HNv2ETKxGx11Rh0hE/w9d9
        hB/xvrgTGNTO87ZzRFzPzBvYQNMTX8JxfMIQo6YMazHOzDsR8QK38K0nXuBliWE8LLe7pq5n5iAi
        nmbmATzClRJv4WyHPxpk5mcc6u36HfcjYj0zhzi2RAxfmn3MGuBuZh6MiAUu4MwyYlNP0seHGvto
        Zl6NiNe42fGkxW7TuUCLHVwu8ju8KmOf94yFeYPZklMt8BbnOsbei4gN3O40mTX129MqHMZJvMH5
        nidPMvMBjtTq04jYbu+5ihP1YTu9B+o2WasLbZbmJ7GCsVKTNH/JwVqbg9/S+L9x/gGNSHqxMF5G
        egAAAABJRU5ErkJggg==
        """;

    private static readonly Texture2D NoTexture = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0f));

    private static int textFieldID = 961;
    private static Texture2D clearButtonTexture;

    private readonly string controlName = $"textField{ID}";

    private GUIContent placeholderContent = GUIContent.none;
    private bool hasPlacholder = false;
    private string placeholder = string.Empty;
    private string value = string.Empty;

    public event EventHandler GainedFocus;

    public event EventHandler LostFocus;

    public event EventHandler ChangedValue;

    public static LazyStyle Style { get; } = new(
        13,
        () => new(GUI.skin.textField)
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new(5, Utility.GetPix(22), 5, 5),
        },
        style => style.padding.right = Utility.GetPix(22));

    public string Value
    {
        get => value;
        set => SetValue(value);
    }

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

    private static Texture2D ClearButtonIcon =>
        clearButtonTexture ? clearButtonTexture : clearButtonTexture = LoadIconFromBase64(EncodedClearButtonImage);

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

    private static LazyStyle ClearButtonStyle { get; } = new(
        13,
        () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            padding = new(0, 0, 5, 5),
            alignment = TextAnchor.MiddleCenter,
            normal = { background = NoTexture },
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

        if (!string.Equals(value, Value, StringComparison.Ordinal))
            SetValue(value);

        if (hasPlacholder && value.Length is 0)
        {
            var textFieldRect = GUILayoutUtility.GetLastRect();

            GUI.Label(textFieldRect, placeholderContent, placeholderStyle);
        }

        if (Value.Length is not 0)
        {
            var textFieldRect = GUILayoutUtility.GetLastRect();
            var clearButtonRect = textFieldRect with { x = textFieldRect.xMax - Utility.GetPix(25f), width = Utility.GetPix(20f), };

            GUI.Box(clearButtonRect, ClearButtonIcon, ClearButtonStyle);

            if (UnityEngine.Input.GetMouseButtonDown(0) && clearButtonRect.Contains(Event.current.mousePosition))
                SetValue(string.Empty);
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

    public void SetValueWithoutNotify(string value) =>
        SetValue(value, false);

    private static Texture2D LoadIconFromBase64(string base64)
    {
        var icon = new Texture2D(16, 16, TextureFormat.RGB24, false);

        icon.LoadImage(Convert.FromBase64String(base64));

        icon.Apply();

        return icon;
    }

    private void SetValue(string value, bool notify = true)
    {
        if (string.Equals(value, this.value, StringComparison.Ordinal))
            return;

        this.value = value ?? string.Empty;

        if (!notify)
            return;

        ChangedValue?.Invoke(this, EventArgs.Empty);
    }
}
