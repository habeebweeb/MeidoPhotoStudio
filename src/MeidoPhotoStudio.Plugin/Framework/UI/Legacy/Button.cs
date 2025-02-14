namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Button(GUIContent content) : BaseControl
{
    private GUIContent content = content ?? new();

    public Button(string label)
        : this(new GUIContent(label ?? string.Empty))
    {
    }

    public Button(Texture icon)
        : this(new GUIContent(icon))
    {
    }

    public static LazyStyle Style { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.button)
        {
            wordWrap = true,
        });

    public string Label
    {
        get => Content.text;
        set => Content.text = value ?? string.Empty;
    }

    public Texture Icon
    {
        get => Content.image;
        set => Content.image = value;
    }

    public GUIContent Content
    {
        get => content;
        set => content = value ?? new();
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        if (GUILayout.Button(content, buttonStyle, layoutOptions))
            OnControlEvent(EventArgs.Empty);
    }
}
