using MeidoPhotoStudio.Plugin.Framework.UI;

using static MeidoPhotoStudio.Plugin.Constants;

namespace MeidoPhotoStudio.Plugin;

public class WindowManager : IManager
{
    private static GUIStyle windowStyle;

    private readonly Dictionary<Window, BaseWindow> windows = [];

    public WindowManager() =>
        ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;

    private static GUIStyle WindowStyle =>
        windowStyle ??= new(GUI.skin.box);

    public BaseWindow this[Window id]
    {
        get => windows[id];
        set => windows[id] = value;
    }

    public void DrawWindow(BaseWindow window)
    {
        if (!window.Visible)
            return;

        window.WindowRect = GUI.Window(window.WindowID, window.WindowRect, window.GUIFunc, string.Empty, WindowStyle);
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

    public bool MouseOverAnyWindow()
    {
        foreach (var window in windows.Values.Where(window => window.Visible))
            if (MouseOverWindow(window))
                return true;

        return false;

        static bool MouseOverWindow(BaseWindow window)
        {
            var mousePosition = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

            return window.WindowRect.Contains(mousePosition);
        }
    }

    private void OnScreenSizeChanged(object sender, EventArgs e)
    {
        foreach (var window in windows.Values)
            window.OnScreenDimensionsChanged(new(Screen.width, Screen.height));
    }
}
