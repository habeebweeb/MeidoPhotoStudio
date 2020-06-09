using System;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class BasePane : BaseControl
    {
        protected List<BaseControl> Controls { get; set; }
        protected List<BasePane> Panes { get; set; }
        protected bool updating = false;

        public BasePane()
        {
            Translation.ReloadTranslationEvent += OnReloadTranslation;
            Controls = new List<BaseControl>();
            Panes = new List<BasePane>();
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
    }
}
