using UnityEngine;

namespace MeidoPhotoStudio.Converter;

public class UI(PluginCore pluginCore)
{
    public bool Visible;

    private const int WindowID = 0xEA4040;
    private const string WindowTitle = Plugin.PluginName + " " + Plugin.PluginVersion;

    private readonly PluginCore core = pluginCore;

    private Rect windowRect;

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
