namespace MeidoPhotoStudio.Plugin;

public class Header(string text) : BaseControl
{
    private static GUIStyle headerLabelStyle;

    private string text = text;
    private GUIContent content = new(text);

    public string Text
    {
        get => text;
        set
        {
            text = value ?? string.Empty;
            content = new(value);
        }
    }

    private static GUIStyle HeaderLabelStyle =>
        headerLabelStyle ??= new(GUI.skin.label)
        {
            padding = new(7, 0, 0, -5),
            normal = { textColor = Color.white },
            fontSize = 14,
        };

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(content, HeaderLabelStyle, layoutOptions);
}
