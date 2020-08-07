using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class EffectPane<T> : BasePane where T : IEffectManager
    {
        protected abstract T EffectManager { get; set; }
        protected Toggle effectToggle;
        protected Button resetEffectButton;
        private bool enabled;
        public override bool Enabled
        {
            get => enabled;
            set
            {
                this.enabled = value;
                this.EffectManager.SetEffectActive(this.enabled);
            }
        }

        public EffectPane(T effectManager) : base()
        {
            this.EffectManager = effectManager;
            this.resetEffectButton = new Button(Translation.Get("effectsPane", "reset"));
            this.resetEffectButton.ControlEvent += (s, a) => this.ResetEffect();
            this.effectToggle = new Toggle(Translation.Get("effectsPane", "onToggle"));
            this.effectToggle.ControlEvent += (s, a) => this.Enabled = this.effectToggle.Value;
        }

        protected override void ReloadTranslation()
        {
            this.updating = true;
            this.effectToggle.Label = Translation.Get("effectsPane", "onToggle");
            this.resetEffectButton.Label = Translation.Get("effectsPane", "reset");
            TranslatePane();
            this.updating = false;
        }

        protected abstract void TranslatePane();

        public override void UpdatePane()
        {
            if (!EffectManager.IsReady) return;
            this.updating = true;
            this.effectToggle.Value = this.EffectManager.IsActive;
            this.UpdateControls();
            this.updating = false;
        }

        protected abstract void UpdateControls();

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            effectToggle.Draw();
            GUILayout.FlexibleSpace();
            GUI.enabled = this.Enabled;
            resetEffectButton.Draw();
            GUILayout.EndHorizontal();
            DrawPane();
            GUI.enabled = true;
        }

        protected abstract void DrawPane();

        private void ResetEffect()
        {
            this.EffectManager.Deactivate();
            this.EffectManager.SetEffectActive(true);
            this.UpdatePane();
        }
    }
}
