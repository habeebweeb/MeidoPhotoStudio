using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PaneHeader(GUIContent content, bool open = true) : BaseControl
{
    private static readonly GUILayoutOption[] LineHeight = [GUILayout.Height(1)];
    private static readonly LazyStyle WhiteLineStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            normal = { background = UIUtility.CreateTexture(2, 2, new(0.75f, 0.75f, 0.75f)) },
            padding = new(0, 0, 1, 1),
            border = new(0, 0, 1, 1),
        });

    private static readonly LazyStyle ArrowStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            padding = new(0, 0, UIUtility.Scaled(5), UIUtility.Scaled(5)),
            alignment = TextAnchor.MiddleCenter,
            normal = { background = Texture2D.blackTexture },
        },
        static style => style.padding = new(0, 0, UIUtility.Scaled(5), UIUtility.Scaled(5)));

    private static readonly LazyStyle ToggleStyle = new(
        StyleSheet.HeadingSize,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            padding = new(UIUtility.Scaled(16), UIUtility.Scaled(16), UIUtility.Scaled(5), UIUtility.Scaled(5)),
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
        },
        static style => style.padding = new(UIUtility.Scaled(16), UIUtility.Scaled(16), UIUtility.Scaled(5), UIUtility.Scaled(5)));

    private HeaderGroup groupController;
    private GUIContent content = content ?? new();
    private bool enabled = open;

    public PaneHeader(string label, bool open = true)
        : this(new GUIContent(label ?? string.Empty), open)
    {
    }

    public string Label
    {
        get => Content.text;
        set => Content.text = value ?? string.Empty;
    }

    public GUIContent Content
    {
        get => content;
        set => content = value ?? new();
    }

    public bool Enabled
    {
        get => enabled;
        set => SetEnabled(value);
    }

    public HeaderGroup Group
    {
        get => groupController;
        set
        {
            groupController?.Deregister(this);
            groupController = value;
            groupController?.Register(this);
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(ToggleStyle, layoutOptions);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions)
    {
        var enabled = GUILayout.Toggle(this.enabled, content, style, layoutOptions);

        var toggleRect = GUILayoutUtility.GetLastRect();
        var leftArrowRect = toggleRect with { x = UIUtility.Scaled(5), width = UIUtility.Scaled(16) };
        var rightArrowRect = leftArrowRect with { x = toggleRect.width - UIUtility.Scaled(21) };

        GUI.Box(leftArrowRect, Enabled ? Symbols.DownTriangle : Symbols.RightTriangle, ArrowStyle);
        GUI.Box(rightArrowRect, Enabled ? Symbols.DownTriangle : Symbols.LeftTriangle, ArrowStyle);

        GUILayout.Box(GUIContent.none, WhiteLineStyle, LineHeight);

        GUILayout.Space(UIUtility.Scaled(3));

        if (enabled != this.enabled)
            Enabled = enabled;
    }

    public void SetEnabledWithoutNotify(bool enabled) =>
        SetEnabled(enabled, false);

    private void SetEnabled(bool enabled, bool notify = true)
    {
        if (this.enabled == enabled)
            return;

        this.enabled = enabled;

        if (!notify)
            return;

        OnControlEvent(EventArgs.Empty);
    }
}
