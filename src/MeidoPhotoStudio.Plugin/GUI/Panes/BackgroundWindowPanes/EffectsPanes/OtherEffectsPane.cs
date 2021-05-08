using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class OtherEffectsPane : BasePane
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
                if (updating)
                    return;

                var value = blurSlider.Value;

                if (!blurEffectManager.Active && value > 0f)
                    blurEffectManager.SetEffectActive(true);
                else if (blurEffectManager.Active && Mathf.Approximately(value, 0f))
                    blurEffectManager.SetEffectActive(false);

                blurEffectManager.BlurSize = value;
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
            updating = true;

            if (sepiaToneEffectManger.Ready)
                sepiaToggle.Value = sepiaToneEffectManger.Active;

            if (blurEffectManager.Ready)
                blurSlider.Value = blurEffectManager.BlurSize;

            updating = false;
        }
    }
}
