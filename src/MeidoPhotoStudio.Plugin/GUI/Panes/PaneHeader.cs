namespace MeidoPhotoStudio.Plugin;

public class PaneHeader(string label, bool open = true) : BaseControl
{
    private static GUIStyle headerLabelStyle;

    private string label = label;
    private GUIContent content = new(label ?? throw new ArgumentNullException(nameof(label)));

    public string Label
    {
        get => label;
        set
        {
            label = string.IsNullOrEmpty(value) ? string.Empty : value;
            content = new(label);
        }
    }

    public bool Enabled { get; set; } = open;

    private static GUIStyle HeaderLabelStyle =>
        headerLabelStyle ??= new(GUI.skin.toggle)
        {
            padding = new(15, 0, 2, 0),
            normal = { textColor = Color.white },
            fontSize = 14,
        };

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        Enabled = GUILayout.Toggle(Enabled, content, HeaderLabelStyle, layoutOptions);

        MpsGui.WhiteLine();
    }
}
