using System;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class BasePane
    {
        protected List<BaseControl> Controls { get; set; }
        protected bool updating = false;
        public virtual bool Visible { get; set; }
        public bool Enabled { get; set; }

        public BasePane()
        {
            Translation.ReloadTranslationEvent += OnReloadTranslation;
            Controls = new List<BaseControl>();
        }

        ~BasePane()
        {
            Translation.ReloadTranslationEvent -= OnReloadTranslation;
        }

        private void OnReloadTranslation(object sender, EventArgs args)
        {
            ReloadTranslation();
        }

        protected virtual void ReloadTranslation() { }

        public virtual void UpdatePane() { }

        public virtual void Draw() { }
    }
}
