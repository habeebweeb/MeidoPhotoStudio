using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BloomPane : EffectPane<BloomEffectManager>
    {
        protected override BloomEffectManager EffectManager { get; set; }
        private Slider intensitySlider;
        private Slider blurSlider;
        private Slider redSlider;
        private Slider greenSlider;
        private Slider blueSlider;
        private Toggle hdrToggle;

        public BloomPane(EffectManager effectManager) : base(effectManager.BloomEffectManager)
        {
            Bloom bloom = GameMain.Instance.MainCamera.GetComponent<Bloom>();

            this.intensitySlider = new Slider(
                Translation.Get("effectBloom", "intensity"), 0f, 5.7f, bloom.bloomIntensity
            );
            this.intensitySlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.Intensity = this.intensitySlider.Value;
            };
            this.blurSlider = new Slider(Translation.Get("effectBloom", "blur"), 0f, 15f, bloom.bloomBlurIterations);
            this.blurSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.BlurIterations = (int)this.blurSlider.Value;
            };
            this.redSlider = new Slider(Translation.Get("backgroundWindow", "red"), 1f, 0.5f, 1f);
            this.redSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.BloomThresholdColorRed = this.redSlider.Value;
            };
            this.greenSlider = new Slider(Translation.Get("backgroundWindow", "green"), 1f, 0.5f, 1f);
            this.greenSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.BloomThresholdColorGreen = this.greenSlider.Value;
            };
            this.blueSlider = new Slider(Translation.Get("backgroundWindow", "blue"), 1f, 0.5f, 1f);
            this.blueSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.BloomThresholdColorBlue = this.blueSlider.Value;
            };
            this.hdrToggle = new Toggle(Translation.Get("effectBloom", "hdrToggle"));
            this.hdrToggle.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.BloomHDR = this.hdrToggle.Value;
            };
        }

        protected override void TranslatePane()
        {
            this.intensitySlider.Label = Translation.Get("effectBloom", "intensity");
            this.blurSlider.Label = Translation.Get("effectBloom", "blur");
            this.redSlider.Label = Translation.Get("backgroundWindow", "red");
            this.greenSlider.Label = Translation.Get("backgroundWindow", "green");
            this.blueSlider.Label = Translation.Get("backgroundWindow", "blue");
            this.hdrToggle.Label = Translation.Get("effectBloom", "hdrToggle");
        }

        protected override void UpdateControls()
        {
            this.intensitySlider.Value = this.EffectManager.Intensity;
            this.blurSlider.Value = this.EffectManager.BlurIterations;
            this.redSlider.Value = this.EffectManager.BloomThresholdColorRed;
            this.greenSlider.Value = this.EffectManager.BloomThresholdColorGreen;
            this.blueSlider.Value = this.EffectManager.BloomThresholdColorBlue;
            this.hdrToggle.Value = this.EffectManager.BloomHDR;
        }

        protected override void DrawPane()
        {
            GUILayoutOption sliderWidth = MiscGUI.HalfSlider;

            GUILayout.BeginHorizontal();
            this.intensitySlider.Draw(sliderWidth);
            this.blurSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.redSlider.Draw(sliderWidth);
            this.greenSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.blueSlider.Draw(sliderWidth);
            GUILayout.FlexibleSpace();
            this.hdrToggle.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
    }
}
