using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PaneHeader(string label, bool open = true) : BaseControl
{
    public const int FontSize = 14;

    private static readonly LazyStyle ToggleStyle = new(
        FontSize,
        () => new(GUI.skin.toggle)
        {
            padding = new(15, 0, 2, 0),
            normal = { textColor = Color.white },
        });

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

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(ToggleStyle, layoutOptions);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions)
    {
        Enabled = GUILayout.Toggle(Enabled, content, style, layoutOptions);

        MpsGui.WhiteLine();
    }
}
