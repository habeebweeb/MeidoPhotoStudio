using System;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class BasePane
    {
        protected BaseWindow parent;
        protected List<BaseControl> Controls { get; set; }
        protected bool updating;
        public virtual bool Visible { get; set; }
        public virtual bool Enabled { get; set; }

        protected BasePane()
        {
            Translation.ReloadTranslationEvent += OnReloadTranslation;
            Controls = new List<BaseControl>();
        }

        ~BasePane() => Translation.ReloadTranslationEvent -= OnReloadTranslation;

        private void OnReloadTranslation(object sender, EventArgs args) => ReloadTranslation();

        public void SetParent(BaseWindow window) => parent = window;

        protected virtual void ReloadTranslation() { }

        public virtual void UpdatePane() { }

        public virtual void Draw() { }
    }
}
