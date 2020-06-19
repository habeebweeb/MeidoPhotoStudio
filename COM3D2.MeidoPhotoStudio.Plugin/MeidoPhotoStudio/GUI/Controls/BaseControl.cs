using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class BaseControl
    {
        public event EventHandler ControlEvent;
        public bool Enabled { get; set; } = true;
        public bool Visible { get; set; } = true;
        public virtual void Draw(params GUILayoutOption[] layoutOptions) { }
        public virtual void Update() { }
        public virtual void Awake() { }
        public virtual void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) { }
        public virtual void OnControlEvent(EventArgs args)
        {
            ControlEvent?.Invoke(this, args);
        }
    }
}
