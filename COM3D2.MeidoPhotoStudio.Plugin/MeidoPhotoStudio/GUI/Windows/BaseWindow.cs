using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class BaseWindow : BasePane
    {
        protected Vector2 scrollPos;
        public BaseWindow() : base() { }
        public virtual void OnGUI(int id)
        {
            Draw();
            GUI.DragWindow();
        }
    }
}
