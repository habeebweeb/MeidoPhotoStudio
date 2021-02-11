using System.Collections.Generic;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    using static Constants;
    public class WindowManager : IManager
    {
        private readonly Dictionary<Window, BaseWindow> Windows = new Dictionary<Window, BaseWindow>();
        public BaseWindow this[Window id]
        {
            get => Windows[id];
            set
            {
                Windows[id] = value;
                Windows[id].Activate();
            }
        }

        public WindowManager() => InputManager.Register(MpsKey.ToggleUI, KeyCode.Tab, "Show/hide all UI");

        public void DrawWindow(BaseWindow window)
        {
            if (window.Visible)
            {
                GUIStyle windowStyle = new GUIStyle(GUI.skin.box);
                window.WindowRect = GUI.Window(window.windowID, window.WindowRect, window.GUIFunc, "", windowStyle);
            }
        }

        public void DrawWindows()
        {
            foreach (BaseWindow window in Windows.Values) DrawWindow(window);
        }

        public void Update()
        {
            foreach (BaseWindow window in Windows.Values) window.Update();
        }

        public void Activate()
        {
            foreach (BaseWindow window in Windows.Values) window.Activate();
        }

        public void Deactivate()
        {
            foreach (BaseWindow window in Windows.Values) window.Deactivate();
        }
    }
}
