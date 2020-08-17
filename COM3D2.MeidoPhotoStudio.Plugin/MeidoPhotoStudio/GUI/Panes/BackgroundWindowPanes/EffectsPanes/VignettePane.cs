using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class VignettePane : EffectPane<VignetteEffectManager>
    {
        protected override VignetteEffectManager EffectManager { get; set; }
        private Slider intensitySlider;
        private Slider blurSlider;
        private Slider blurSpreadSlider;
        private Slider aberrationSlider;

        public VignettePane(EffectManager effectManager) : base(effectManager.Get<VignetteEffectManager>())
        {
            this.intensitySlider = new Slider(Translation.Get("effectVignette", "intensity"), -40f, 70f);
            this.intensitySlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.Intensity = this.intensitySlider.Value;
            };
            this.blurSlider = new Slider(Translation.Get("effectVignette", "blur"), 0f, 5f);
            this.blurSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.Blur = this.blurSlider.Value;
            };
            this.blurSpreadSlider = new Slider(Translation.Get("effectVignette", "blurSpread"), 0f, 40f);
            this.blurSpreadSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.BlurSpread = this.blurSpreadSlider.Value;
            };
            this.aberrationSlider = new Slider(Translation.Get("effectVignette", "aberration"), -30f, 30f);
            this.aberrationSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.ChromaticAberration = this.aberrationSlider.Value;
            };
        }

        protected override void TranslatePane()
        {
            this.intensitySlider.Label = Translation.Get("effectVignette", "intensity");
            this.blurSlider.Label = Translation.Get("effectVignette", "blur");
            this.blurSpreadSlider.Label = Translation.Get("effectVignette", "blurSpread");
            this.aberrationSlider.Label = Translation.Get("effectVignette", "aberration");
        }

        protected override void UpdateControls()
        {
            this.intensitySlider.Value = this.EffectManager.Intensity;
            this.blurSlider.Value = this.EffectManager.Blur;
            this.blurSpreadSlider.Value = this.EffectManager.BlurSpread;
            this.aberrationSlider.Value = this.EffectManager.ChromaticAberration;
        }

        protected override void DrawPane()
        {
            GUILayoutOption sliderWidth = MiscGUI.HalfSlider;

            GUILayout.BeginHorizontal();
            this.intensitySlider.Draw(sliderWidth);
            this.blurSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.blurSpreadSlider.Draw(sliderWidth);
            this.aberrationSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();
        }
    }
}
