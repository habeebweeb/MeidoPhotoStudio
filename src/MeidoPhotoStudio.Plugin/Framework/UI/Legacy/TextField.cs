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

    private static int textFieldID = 961;
    private static Texture2D clearButtonTexture;

    private readonly string controlName = $"textField{ID}";

    private GUIContent placeholderContent;
    private bool hasPlaceholder = false;
    private string value = string.Empty;

    public TextField()
    {
    }

    public TextField(string placeholder)
        : this(new GUIContent(placeholder ?? string.Empty))
    {
    }

    public TextField(GUIContent placeholderContent) =>
        PlaceholderContent = placeholderContent ?? new();

    public event EventHandler GainedFocus;

    public event EventHandler LostFocus;

    public event EventHandler ChangedValue;

    public static LazyStyle Style { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.textField)
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new(5, UIUtility.Scaled(22), 5, 5),
        },
        static style => style.padding.right = UIUtility.Scaled(22));

    public string Value
    {
        get => value;
        set => SetValue(value);
    }

    public string Placeholder
    {
        get => placeholderContent?.text;
        set
        {
            if (value is null)
            {
                PlaceholderContent = null;

                return;
            }

            PlaceholderContent ??= new();
            PlaceholderContent.text = value;
        }
    }

    public GUIContent PlaceholderContent
    {
        get => placeholderContent;
        set
        {
            placeholderContent = value;
            hasPlaceholder = placeholderContent is not null;
        }
    }

    public bool HasFocus { get; private set; }

    private static Texture2D ClearButtonIcon =>
        clearButtonTexture ? clearButtonTexture : clearButtonTexture = UIUtility.LoadTextureFromBase64(16, 16, EncodedClearButtonImage);

    private static LazyStyle PlaceholderStyle { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            padding = new(5, 5, 0, 0),
            alignment = TextAnchor.MiddleLeft,
            normal =
            {
                textColor = new(1f, 1f, 1f, 0.6f),
            },
        });

    private static LazyStyle ClearButtonStyle { get; } = new(
        0,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            padding = new(0, 0, 5, 5),
            alignment = TextAnchor.MiddleCenter,
            normal = { background = Texture2D.blackTexture },
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

        if (hasPlaceholder && value.Length is 0)
        {
            var textFieldRect = GUILayoutUtility.GetLastRect();

            GUI.Label(textFieldRect, placeholderContent, placeholderStyle);
        }

        if (Value.Length is not 0)
        {
            var textFieldRect = GUILayoutUtility.GetLastRect();
            var clearButtonRect = textFieldRect with { x = textFieldRect.xMax - UIUtility.Scaled(25f), width = UIUtility.Scaled(20f), };

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
