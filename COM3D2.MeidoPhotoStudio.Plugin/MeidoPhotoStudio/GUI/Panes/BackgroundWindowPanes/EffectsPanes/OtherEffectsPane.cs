using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class OtherEffectsPane : BasePane
    {
        private readonly EffectManager effectManager;
        private readonly SepiaToneEffectManger sepiaToneEffectManger;
        private readonly BlurEffectManager blurEffectManager;
        private readonly Toggle sepiaToggle;
        private readonly Slider blurSlider;

        public OtherEffectsPane(EffectManager effectManager)
        {
            this.effectManager = effectManager;

            sepiaToneEffectManger = this.effectManager.Get<SepiaToneEffectManger>();
            blurEffectManager = this.effectManager.Get<BlurEffectManager>();

            sepiaToggle = new Toggle(Translation.Get("otherEffectsPane", "sepiaToggle"));
            sepiaToggle.ControlEvent += (s, a) =>
            {
                if (updating) return;
                sepiaToneEffectManger.SetEffectActive(sepiaToggle.Value);
            };

            blurSlider = new Slider(Translation.Get("otherEffectsPane", "blurSlider"), 0f, 18f);
            blurSlider.ControlEvent += (s, a) =>
            {
                float value = blurSlider.Value;
                if (!blurEffectManager.Active && value > 0f) blurEffectManager.SetEffectActive(true);
                else if (blurEffectManager.Active && value == 0f) blurEffectManager.SetEffectActive(false);

                if (blurEffectManager.Active) blurEffectManager.BlurSize = blurSlider.Value;
            };
        }

        protected override void ReloadTranslation()
        {
            sepiaToggle.Label = Translation.Get("otherEffectsPane", "sepiaToggle");
            blurSlider.Label = Translation.Get("otherEffectsPane", "blurSlider");
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            sepiaToggle.Draw();
            blurSlider.Draw();
            GUILayout.EndHorizontal();
        }

        public override void UpdatePane()
        {
            if (sepiaToneEffectManger.Ready)
            {
                updating = true;
                sepiaToggle.Value = sepiaToneEffectManger.Active;
                updating = false;
            }

            if (blurEffectManager.Ready) blurSlider.Value = blurEffectManager.BlurSize;
        }
    }
}
