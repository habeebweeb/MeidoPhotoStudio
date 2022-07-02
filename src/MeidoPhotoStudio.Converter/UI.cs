using UnityEngine;

namespace MeidoPhotoStudio.Converter
{
    public class UI
    {
        private const int WindowID = 0xEA4040;
        private const string WindowTitle = Plugin.PluginName + " " + Plugin.PluginVersion;
        private Rect windowRect;

        private PluginCore core;

        public bool Visible;

        public UI(PluginCore pluginCore) =>
            core = pluginCore;

        public void Draw()
        {
            if (!Visible)
                return;

            windowRect.width = 230f;
            windowRect.height = 100f;
            windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
            windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);
            windowRect = GUI.Window(WindowID, windowRect, GUIFunc, WindowTitle);
        }

        private void GUIFunc(int windowId)
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Convert"))
                core.Convert();

            GUI.DragWindow();
        }
    }
}
