using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class SubPaneHeader(GUIContent content, bool open = true) : BaseControl
{
    private const string ClosedArrowBase64 =
        """
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAiElEQVQ4y73TuxFBYRRF4X88QkMH
        t4vbBxFVaEERZFcXREKRRKIIFRgRn8AJzI3M8VgFrJmz9z6lfBPscUSVFRw8OaPOCAbYhOSKWUbS
        xTIkdyyy58xxC9Ea/YxkjEtIdhhlJHWECqd2Q53ySzBpnTDMhti8HSJ6WKVqjCFtX4Y0/fuUP3um
        DA9NBNUNhOsvFAAAAABJRU5ErkJggg==
        """;

    private const string OpenedArrowBase64 =
        """
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAmElEQVQ4y93SvQnCYBAG4BRJKXGD
        FI5g4xxWLqKjCAEXiW1K01g4g4Kt2Olj4QdGMYmxzAvX3PtzB3dRNAygxA7pD9o0aMt6s/LEFkmL
        OQkaqOpEhlMgNi0B66A5Y/JJTnEJguUX8ypwV8yaJsxxwx2Lrn5TyNukrs2aQvJgOIaCvM9ZYxRe
        KBD3/Y0R9jhg/O+DZciiYeMBkHHnEe6GMtoAAAAASUVORK5CYII=
        """;

    private static readonly LazyStyle ArrowStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            padding = new(UIUtility.Scaled(5), UIUtility.Scaled(5), UIUtility.Scaled(5), UIUtility.Scaled(5)),
            alignment = TextAnchor.MiddleCenter,
            normal = { background = Texture2D.blackTexture },
        },
        static style => style.padding = new(UIUtility.Scaled(5), UIUtility.Scaled(5), UIUtility.Scaled(5), UIUtility.Scaled(5)));

    private static readonly LazyStyle ToggleStyle = new(
        StyleSheet.SubHeadingSize,
        static () => new(GUI.skin.label)
        {
            margin = new(0, 0, 0, 0),
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            contentOffset = new(UIUtility.Scaled(20), 0f),
        },
        static style => style.contentOffset = new(UIUtility.Scaled(20), 0f));

    private static readonly GUIContent ClosedArrow = new(UIUtility.LoadTextureFromBase64(16, 16, ClosedArrowBase64));
    private static readonly GUIContent OpenedArrow = new(UIUtility.LoadTextureFromBase64(16, 16, OpenedArrowBase64));

    private GUIContent content = content ?? new();

    public SubPaneHeader(string label, bool open = true)
        : this(new GUIContent(label ?? string.Empty), open)
    {
    }

    public string Label
    {
        get => content.text;
        set => content.text = value ?? string.Empty;
    }

    public GUIContent Content
    {
        get => content;
        set => content = value ?? new();
    }

    public bool Enabled { get; set; } = open;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(ToggleStyle, layoutOptions);

    public void Draw(GUIStyle style, params GUILayoutOption[] layoutOptions)
    {
        Enabled = GUILayout.Toggle(Enabled, content, style, layoutOptions);

        var toggleRect = GUILayoutUtility.GetLastRect();
        var arrowRect = toggleRect with { x = 0f, width = UIUtility.Scaled(25) };

        GUI.Box(arrowRect, Enabled ? OpenedArrow : ClosedArrow, ArrowStyle);
    }
}
