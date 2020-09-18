using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class BaseControl
    {
        public event EventHandler ControlEvent;
        public virtual void Draw(params GUILayoutOption[] layoutOptions) { }
        public virtual void OnControlEvent(EventArgs args) => ControlEvent?.Invoke(this, args);
    }
}
