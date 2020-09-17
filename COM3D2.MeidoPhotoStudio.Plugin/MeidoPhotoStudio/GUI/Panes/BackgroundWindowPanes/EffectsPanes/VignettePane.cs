using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class VignettePane : EffectPane<VignetteEffectManager>
    {
        protected override VignetteEffectManager EffectManager { get; set; }
        private readonly Slider intensitySlider;
        private readonly Slider blurSlider;
        private readonly Slider blurSpreadSlider;
        private readonly Slider aberrationSlider;

        public VignettePane(EffectManager effectManager) : base(effectManager.Get<VignetteEffectManager>())
        {
            intensitySlider = new Slider(Translation.Get("effectVignette", "intensity"), -40f, 70f);
            intensitySlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.Intensity = intensitySlider.Value;
            };
            blurSlider = new Slider(Translation.Get("effectVignette", "blur"), 0f, 5f);
            blurSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.Blur = blurSlider.Value;
            };
            blurSpreadSlider = new Slider(Translation.Get("effectVignette", "blurSpread"), 0f, 40f);
            blurSpreadSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.BlurSpread = blurSpreadSlider.Value;
            };
            aberrationSlider = new Slider(Translation.Get("effectVignette", "aberration"), -30f, 30f);
            aberrationSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.ChromaticAberration = aberrationSlider.Value;
            };
        }

        protected override void TranslatePane()
        {
            intensitySlider.Label = Translation.Get("effectVignette", "intensity");
            blurSlider.Label = Translation.Get("effectVignette", "blur");
            blurSpreadSlider.Label = Translation.Get("effectVignette", "blurSpread");
            aberrationSlider.Label = Translation.Get("effectVignette", "aberration");
        }

        protected override void UpdateControls()
        {
            intensitySlider.Value = EffectManager.Intensity;
            blurSlider.Value = EffectManager.Blur;
            blurSpreadSlider.Value = EffectManager.BlurSpread;
            aberrationSlider.Value = EffectManager.ChromaticAberration;
        }

        protected override void DrawPane()
        {
            GUILayoutOption sliderWidth = MpsGui.HalfSlider;

            GUILayout.BeginHorizontal();
            intensitySlider.Draw(sliderWidth);
            blurSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            blurSpreadSlider.Draw(sliderWidth);
            aberrationSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();
        }
    }
}
