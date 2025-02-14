namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Header(GUIContent content) : BaseControl
{
    private GUIContent content = content ?? new();

    public Header(string text)
        : this(new GUIContent(text ?? string.Empty))
    {
    }

    public static LazyStyle Style { get; } = new(
        StyleSheet.SubHeadingSize,
        static () => new(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            wordWrap = true,
            contentOffset = new(5f, 0f),
        });

    public string Text
    {
        get => Content.text;
        set => Content.text = value ?? string.Empty;
    }

    public GUIContent Content
    {
        get => content;
        set => content = value ?? new();
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(content, style, layoutOptions);
}
