using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public static class MpsGui
{
    public static readonly GUILayoutOption HalfSlider = GUILayout.Width(98);
    public static readonly Texture2D White = Utility.MakeTex(2, 2, Color.white);
    public static readonly Texture2D TransparentBlack = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.8f));
    public static readonly GUIStyle SliderLabelStyle;
    public static readonly GUIStyle SliderStyle;
    public static readonly GUIStyle SliderStyleNoLabel;
    public static readonly GUIStyle SliderTextBoxStyle;
    public static readonly GUIStyle SliderThumbStyle;
    public static readonly GUIStyle SliderResetButtonStyle;

    private static readonly GUIStyle LineStyleWhite;
    private static readonly GUIStyle LineStyleBlack;
    private static readonly GUIStyle TextureBoxStyle;
    private static readonly GUIStyle HeaderLabelStyle;

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

        HeaderLabelStyle = new(GUI.skin.label)
        {
            padding = new(7, 0, 0, -5),
            normal = { textColor = Color.white },
            fontSize = 14,
        };

        SliderLabelStyle = new(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
            fontSize = 13,
            normal = { textColor = Color.white },
        };

        SliderStyle = new(GUI.skin.horizontalSlider);
        SliderStyleNoLabel = new(SliderStyle)
        {
            margin = { top = 10 },
        };

        SliderTextBoxStyle = new(GUI.skin.textField)
        {
            fontSize = 12,
        };

        SliderResetButtonStyle = new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleRight,
            fontSize = 10,
        };

        SliderThumbStyle = new(GUI.skin.horizontalSliderThumb);
    }

    public static void WhiteLine() =>
        Line(LineStyleWhite);

    public static void BlackLine() =>
        Line(LineStyleBlack);

    public static void DrawTexture(Texture texture, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Box(texture, TextureBoxStyle, layoutOptions);

    public static int ClampFont(int size, int min, int max) =>
        Mathf.Clamp(Utility.GetPix(size), min, max);

    public static void Header(string text, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(text, HeaderLabelStyle, layoutOptions);

    private static void Line(GUIStyle style) =>
        GUILayout.Box(GUIContent.none, style, GUILayout.Height(1));
}
