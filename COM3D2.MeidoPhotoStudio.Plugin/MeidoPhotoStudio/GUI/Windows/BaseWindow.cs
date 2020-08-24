using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class BaseWindow : BaseWindowPane
    {
        private static int id = 765;
        private static int ID => id++;
        public readonly int windowID;
        protected Rect windowRect = new Rect(0f, 0f, 480f, 270f);
        public virtual Rect WindowRect
        {
            get => windowRect;
            set
            {
                value.x = Mathf.Clamp(
                    value.x, -value.width + Utility.GetPix(20), Screen.width - Utility.GetPix(20)
                );
                value.y = Mathf.Clamp(
                    value.y, -value.height + Utility.GetPix(20), Screen.height - Utility.GetPix(20)
                );
                windowRect = value;
            }
        }
        protected Vector2 MiddlePosition => new Vector2(
            Screen.width / 2 - windowRect.width / 2,
            Screen.height / 2 - windowRect.height / 2
        );

        public BaseWindow ModalWindow { get; private set; }

        public BaseWindow() : base() => windowID = ID;

        public virtual void HandleZoom()
        {
            if (Input.mouseScrollDelta.y != 0f)
            {
                if (Visible)
                {
                    Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                    if (WindowRect.Contains(mousePos)) Input.ResetInputAxes();
                }
            }
        }

        public virtual void Update() => HandleZoom();

        public virtual void Activate() { }

        public virtual void Deactivate() { }

        public virtual void GUIFunc(int id)
        {
            Draw();
            GUI.DragWindow();
        }
    }
}
