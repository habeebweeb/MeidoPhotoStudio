using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public static class MpsGui
    {
        public static readonly GUILayoutOption HalfSlider = GUILayout.Width(98);
        public static readonly Texture2D white = Utility.MakeTex(2, 2, Color.white);
        public static readonly Texture2D transparentBlack = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
        public static readonly GUIStyle SliderLabelStyle;
        public static readonly GUIStyle SliderStyle;
        public static readonly GUIStyle SliderStyleNoLabel;
        public static readonly GUIStyle SliderTextBoxStyle;
        public static readonly GUIStyle SliderThumbStyle;
        public static readonly GUIStyle SliderResetButtonStyle;
        private static readonly GUIStyle lineStyleWhite;
        private static readonly GUIStyle lineStyleBlack;
        private static readonly GUIStyle textureBoxStyle;
        private static readonly GUIStyle headerLabelStyle;

        static MpsGui()
        {
            GUI.skin = null;

            lineStyleWhite = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(0, 0, 8, 8),
                normal = { background = Utility.MakeTex(2, 2, new Color(1f, 1f, 1f, 0.2f)) }
            };
            lineStyleWhite.padding = lineStyleWhite.border = new RectOffset(0, 0, 1, 1);

            lineStyleBlack = new GUIStyle(lineStyleWhite)
            {
                normal = { background = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.3f)) }
            };

            textureBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0f)) }
            };
            textureBoxStyle.padding = textureBoxStyle.margin = new RectOffset(0, 0, 0, 0);

            headerLabelStyle = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(7, 0, 0, -5),
                normal = { textColor = Color.white },
                fontSize = 14
            };

            SliderLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerLeft,
                fontSize = 13,
                normal = { textColor = Color.white }
            };
            SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            SliderStyleNoLabel = new GUIStyle(SliderStyle) { margin = { top = 10 } };
            SliderTextBoxStyle = new GUIStyle(GUI.skin.textField) { fontSize = 12, };
            SliderResetButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 10
            };
            SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        }

        private static void Line(GUIStyle style) => GUILayout.Box(GUIContent.none, style, GUILayout.Height(1));

        public static void WhiteLine() => Line(lineStyleWhite);

        public static void BlackLine() => Line(lineStyleBlack);

        public static void DrawTexture(Texture texture, params GUILayoutOption[] layoutOptions)
        {
            GUILayout.Box(texture, textureBoxStyle, layoutOptions);
        }

        public static int ClampFont(int size, int min, int max) => Mathf.Clamp(Utility.GetPix(size), min, max);

        public static void Header(string text, params GUILayoutOption[] layoutOptions)
        {
            GUILayout.Label(text, headerLabelStyle, layoutOptions);
        }
    }
}
