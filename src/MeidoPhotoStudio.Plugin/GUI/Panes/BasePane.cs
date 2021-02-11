using System;

namespace MeidoPhotoStudio.Plugin
{
    public abstract class BasePane
    {
        protected BaseWindow parent;
        protected bool updating;
        public virtual bool Visible { get; set; }
        public virtual bool Enabled { get; set; }

        protected BasePane() => Translation.ReloadTranslationEvent += OnReloadTranslation;

        ~BasePane() => Translation.ReloadTranslationEvent -= OnReloadTranslation;

        private void OnReloadTranslation(object sender, EventArgs args) => ReloadTranslation();

        public virtual void SetParent(BaseWindow window) => parent = window;

        protected virtual void ReloadTranslation() { }

        public virtual void UpdatePane() { }

        public virtual void Draw() { }
    }
}
