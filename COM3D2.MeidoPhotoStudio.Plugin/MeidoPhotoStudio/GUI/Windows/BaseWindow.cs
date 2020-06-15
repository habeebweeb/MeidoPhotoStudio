using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class BaseWindow : BaseWindowPane
    {
        private static int id = 765;
        private static int ID { get => id++; }
        public readonly int windowID;
        protected Rect windowRect;
        public abstract Rect WindowRect { get; set; }

        public BaseWindow() : base()
        {
            windowID = ID;
        }

        protected virtual void HandleZoom()
        {
            if (Input.mouseScrollDelta.y != 0f)
            {
                if (Visible && WindowRect.Contains(Event.current.mousePosition))
                {
                    Input.ResetInputAxes();
                }
            }
        }

        public virtual void Update()
        {
            HandleZoom();
        }

        public virtual void Activate() { }

        public virtual void Deactivate() { }

        public virtual void GUIFunc(int id)
        {
            Draw();
            GUI.DragWindow();
        }
    }
}
