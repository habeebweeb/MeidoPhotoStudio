using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class MiscGUI
    {
        public static readonly GUILayoutOption HalfSlider = GUILayout.Width(94);
        private static GUIStyle lineStyleWhite;
        private static GUIStyle lineStyleBlack;
        private static GUIStyle textureBoxStyle;
        static MiscGUI()
        {
            lineStyleWhite = new GUIStyle(GUI.skin.box);
            lineStyleWhite.padding = lineStyleWhite.border = new RectOffset(0, 0, 1, 1);
            lineStyleWhite.margin = new RectOffset(0, 0, 8, 8);
            lineStyleWhite.normal.background = Utility.MakeTex(2, 2, new Color(1f, 1f, 1f, 0.2f));

            lineStyleBlack = new GUIStyle(lineStyleWhite);
            lineStyleBlack.normal.background = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.3f));

            textureBoxStyle = new GUIStyle(GUI.skin.box);
            textureBoxStyle.normal.background = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0f));
        }

        private static void Line(GUIStyle style) => GUILayout.Box(GUIContent.none, style, GUILayout.Height(1));

        public static void WhiteLine() => Line(lineStyleWhite);

        public static void BlackLine() => Line(lineStyleBlack);

        public static void DrawTexture(Texture texture, params GUILayoutOption[] layoutOptions)
        {
            GUILayout.Box(texture, textureBoxStyle, layoutOptions);
        }

        public static void Header(string text, params GUILayoutOption[] layoutOptions)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.padding = new RectOffset(7, 0, 0, -5);

            GUILayout.Label(text, style, layoutOptions);
        }
    }
}
