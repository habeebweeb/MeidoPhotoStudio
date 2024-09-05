namespace MeidoPhotoStudio.Plugin;

public static class MpsGui
{
    public static readonly GUILayoutOption HalfSlider = GUILayout.Width(98);
    public static readonly Texture2D White = Utility.MakeTex(2, 2, Color.white);
    public static readonly Texture2D TransparentBlack = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.8f));

    private static readonly GUIStyle LineStyleWhite;
    private static readonly GUIStyle LineStyleBlack;
    private static readonly GUIStyle TextureBoxStyle;

    static MpsGui()
    {
        GUI.skin = null;

        LineStyleWhite = new(GUI.skin.box)
        {
            margin = new(0, 0, 8, 8),
            normal = { background = Utility.MakeTex(2, 2, new(1f, 1f, 1f, 0.2f)) },
        };

        LineStyleWhite.padding = LineStyleWhite.border = new(0, 0, 1, 1);

        LineStyleBlack = new(LineStyleWhite)
        {
            normal = { background = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.3f)) },
        };

        TextureBoxStyle = new(GUI.skin.box)
        {
            normal = { background = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0f)) },
        };

        TextureBoxStyle.padding = TextureBoxStyle.margin = new(0, 0, 0, 0);
    }

    public static void WhiteLine() =>
        Line(LineStyleWhite);

    public static void BlackLine() =>
        Line(LineStyleBlack);

    public static void DrawTexture(Texture texture, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Box(texture, TextureBoxStyle, layoutOptions);

    public static int ClampFont(int size, int min, int max) =>
        Mathf.Clamp(Utility.GetPix(size), min, max);

    private static void Line(GUIStyle style) =>
        GUILayout.Box(GUIContent.none, style, GUILayout.Height(1));
}
