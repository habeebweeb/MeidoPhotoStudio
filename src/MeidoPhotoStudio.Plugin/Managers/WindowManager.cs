using System.Collections.Generic;

using UnityEngine;

using static MeidoPhotoStudio.Plugin.Constants;

namespace MeidoPhotoStudio.Plugin;

public class WindowManager : IManager
{
    private readonly Dictionary<Window, BaseWindow> windows = new();

    public BaseWindow this[Window id]
    {
        get => windows[id];
        set
        {
            windows[id] = value;
            windows[id].Activate();
        }
    }

    public void DrawWindow(BaseWindow window)
    {
        if (!window.Visible)
            return;

        var windowStyle = new GUIStyle(GUI.skin.box);

        window.WindowRect = GUI.Window(window.WindowID, window.WindowRect, window.GUIFunc, string.Empty, windowStyle);
    }

    public void DrawWindows()
    {
        foreach (var window in windows.Values)
            DrawWindow(window);
    }

    public void Update()
    {
        foreach (var window in windows.Values)
            window.Update();
    }

    public void Activate()
    {
        foreach (var window in windows.Values)
            window.Activate();
    }

    public void Deactivate()
    {
        foreach (var window in windows.Values)
            window.Deactivate();
    }
}
