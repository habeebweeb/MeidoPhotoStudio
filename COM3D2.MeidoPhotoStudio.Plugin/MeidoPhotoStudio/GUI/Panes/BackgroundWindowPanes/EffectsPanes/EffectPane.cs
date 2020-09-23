using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class EffectPane<T> : BasePane where T : IEffectManager
    {
        protected abstract T EffectManager { get; set; }
        protected readonly Toggle effectToggle;
        protected readonly Button resetEffectButton;
        private bool enabled;
        public override bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                if (updating) return;
                EffectManager.SetEffectActive(enabled);
            }
        }

        protected EffectPane(EffectManager effectManager)
        {
            EffectManager = effectManager.Get<T>();
            resetEffectButton = new Button(Translation.Get("effectsPane", "reset"));
            resetEffectButton.ControlEvent += (s, a) => ResetEffect();
            effectToggle = new Toggle(Translation.Get("effectsPane", "onToggle"));
            effectToggle.ControlEvent += (s, a) => Enabled = effectToggle.Value;
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            effectToggle.Label = Translation.Get("effectsPane", "onToggle");
            resetEffectButton.Label = Translation.Get("effectsPane", "reset");
            TranslatePane();
            updating = false;
        }

        protected abstract void TranslatePane();

        public override void UpdatePane()
        {
            if (!EffectManager.Ready) return;
            updating = true;
            effectToggle.Value = EffectManager.Active;
            UpdateControls();
            updating = false;
        }

        protected abstract void UpdateControls();

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            effectToggle.Draw();
            GUILayout.FlexibleSpace();
            GUI.enabled = Enabled;
            resetEffectButton.Draw();
            GUILayout.EndHorizontal();
            DrawPane();
            GUI.enabled = true;
        }

        protected abstract void DrawPane();

        private void ResetEffect()
        {
            EffectManager.Deactivate();
            EffectManager.SetEffectActive(true);
            UpdatePane();
        }
    }
}
