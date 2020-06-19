using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static Constants;
    internal class WindowManager
    {
        private Dictionary<Window, BaseWindow> Windows = new Dictionary<Window, BaseWindow>();
        private List<BaseWindow> WindowList = new List<BaseWindow>();
        public BaseWindow this[Window id]
        {
            get => Windows[id];
            set
            {
                Windows[id] = value;
                WindowList.Add(Windows[id]);
            }
        }

        public bool AddWindow(Window id, BaseWindow window)
        {
            if (!this.Windows.ContainsKey(id))
            {
                this.Windows[id] = window;
                this.WindowList.Add(window);
                return true;
            }
            return false;
        }

        public bool RemoveWindow(Window id)
        {
            if (Windows.ContainsKey(id))
            {
                WindowList.Remove(Windows[id]);
                Windows.Remove(id);
                return true;
            }
            return false;
        }

        public void DrawWindow(Window id)
        {
            DrawWindow(Windows[id]);
        }

        public void DrawWindow(BaseWindow window)
        {
            if (window.Visible)
            {
                GUIStyle windowStyle = new GUIStyle(GUI.skin.box);
                window.WindowRect = GUI.Window(window.windowID, window.WindowRect, window.GUIFunc, "", windowStyle);
            }

            if (DropdownHelper.Visible) DropdownHelper.HandleDropdown();
        }

        public void DrawWindows()
        {
            foreach (BaseWindow window in WindowList)
            {
                DrawWindow(window);
            }
        }

        public void Update()
        {
            foreach (BaseWindow window in WindowList)
            {
                window.Update();
            }
        }

        public void Activate()
        {
            foreach (BaseWindow window in WindowList)
            {
                window.Activate();
            }
        }

        public void Deactivate()
        {
            foreach (BaseWindow window in WindowList)
            {
                window.Deactivate();
            }
        }
    }
}
