using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BloomPane : EffectPane<BloomEffectManager>
    {
        protected override BloomEffectManager EffectManager { get; set; }
        private readonly Slider intensitySlider;
        private readonly Slider blurSlider;
        private readonly Slider redSlider;
        private readonly Slider greenSlider;
        private readonly Slider blueSlider;
        private readonly Toggle hdrToggle;

        public BloomPane(EffectManager effectManager) : base(effectManager.Get<BloomEffectManager>())
        {
            Bloom bloom = GameMain.Instance.MainCamera.GetComponent<Bloom>();

            intensitySlider = new Slider(
                Translation.Get("effectBloom", "intensity"), 0f, 5.7f, bloom.bloomIntensity
            );
            intensitySlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.Intensity = intensitySlider.Value;
            };
            blurSlider = new Slider(Translation.Get("effectBloom", "blur"), 0f, 15f, bloom.bloomBlurIterations);
            blurSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.BlurIterations = (int)blurSlider.Value;
            };
            redSlider = new Slider(Translation.Get("backgroundWindow", "red"), 1f, 0.5f, 1f);
            redSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.BloomThresholdColorRed = redSlider.Value;
            };
            greenSlider = new Slider(Translation.Get("backgroundWindow", "green"), 1f, 0.5f, 1f);
            greenSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.BloomThresholdColorGreen = greenSlider.Value;
            };
            blueSlider = new Slider(Translation.Get("backgroundWindow", "blue"), 1f, 0.5f, 1f);
            blueSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.BloomThresholdColorBlue = blueSlider.Value;
            };
            hdrToggle = new Toggle(Translation.Get("effectBloom", "hdrToggle"));
            hdrToggle.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.BloomHDR = hdrToggle.Value;
            };
        }

        protected override void TranslatePane()
        {
            intensitySlider.Label = Translation.Get("effectBloom", "intensity");
            blurSlider.Label = Translation.Get("effectBloom", "blur");
            redSlider.Label = Translation.Get("backgroundWindow", "red");
            greenSlider.Label = Translation.Get("backgroundWindow", "green");
            blueSlider.Label = Translation.Get("backgroundWindow", "blue");
            hdrToggle.Label = Translation.Get("effectBloom", "hdrToggle");
        }

        protected override void UpdateControls()
        {
            intensitySlider.Value = EffectManager.Intensity;
            blurSlider.Value = EffectManager.BlurIterations;
            redSlider.Value = EffectManager.BloomThresholdColorRed;
            greenSlider.Value = EffectManager.BloomThresholdColorGreen;
            blueSlider.Value = EffectManager.BloomThresholdColorBlue;
            hdrToggle.Value = EffectManager.BloomHDR;
        }

        protected override void DrawPane()
        {
            GUILayoutOption sliderWidth = MiscGUI.HalfSlider;

            GUILayout.BeginHorizontal();
            intensitySlider.Draw(sliderWidth);
            blurSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            redSlider.Draw(sliderWidth);
            greenSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            blueSlider.Draw(sliderWidth);
            GUILayout.FlexibleSpace();
            hdrToggle.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
    }
}
