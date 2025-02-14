namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Label(GUIContent content) : BaseControl
{
    private GUIContent content = content ?? new();

    public Label(string text)
        : this(new GUIContent(text ?? string.Empty))
    {
    }

    public static LazyStyle Style { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            wordWrap = true,
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

    public virtual void Draw(GUIStyle labelStyle, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(content, labelStyle, layoutOptions);
}
