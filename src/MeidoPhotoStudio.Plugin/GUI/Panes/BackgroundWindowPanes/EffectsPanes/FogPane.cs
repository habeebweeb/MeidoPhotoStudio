using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class FogPane : EffectPane<FogEffectManager>
    {
        protected override FogEffectManager EffectManager { get; set; }
        private readonly Slider distanceSlider;
        private readonly Slider densitySlider;
        private readonly Slider heightScaleSlider;
        private readonly Slider heightSlider;
        private readonly Slider redSlider;
        private readonly Slider greenSlider;
        private readonly Slider blueSlider;

        public FogPane(EffectManager effectManager) : base(effectManager)
        {
            distanceSlider = new Slider(
                Translation.Get("effectFog", "distance"), 0f, 30f, EffectManager.Distance
            );
            densitySlider = new Slider(
                Translation.Get("effectFog", "density"), 0f, 10f, EffectManager.Density
            );
            heightScaleSlider = new Slider(
                Translation.Get("effectFog", "strength"), -5f, 20f, EffectManager.HeightScale
            );
            heightSlider = new Slider(
                Translation.Get("effectFog", "height"), -10f, 10f, EffectManager.Height
            );
            Color initialFogColour = EffectManager.FogColour;
            redSlider = new Slider(Translation.Get("backgroundWIndow", "red"), 0f, 1f, initialFogColour.r);
            greenSlider = new Slider(Translation.Get("backgroundWIndow", "green"), 0f, 1f, initialFogColour.g);
            blueSlider = new Slider(Translation.Get("backgroundWIndow", "blue"), 0f, 1f, initialFogColour.b);
            distanceSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.Distance = distanceSlider.Value;
            };
            densitySlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.Density = densitySlider.Value;
            };
            heightScaleSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.HeightScale = heightScaleSlider.Value;
            };
            heightSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.Height = heightSlider.Value;
            };
            redSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.FogColourRed = redSlider.Value;
            };
            greenSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.FogColourGreen = greenSlider.Value;
            };
            blueSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                EffectManager.FogColourBlue = blueSlider.Value;
            };
        }

        protected override void TranslatePane()
        {
            distanceSlider.Label = Translation.Get("effectFog", "distance");
            densitySlider.Label = Translation.Get("effectFog", "density");
            heightScaleSlider.Label = Translation.Get("effectFog", "strength");
            heightSlider.Label = Translation.Get("effectFog", "height");
            redSlider.Label = Translation.Get("backgroundWIndow", "red");
            greenSlider.Label = Translation.Get("backgroundWIndow", "green");
            blueSlider.Label = Translation.Get("backgroundWIndow", "blue");
        }

        protected override void UpdateControls()
        {
            distanceSlider.Value = EffectManager.Distance;
            densitySlider.Value = EffectManager.Density;
            heightScaleSlider.Value = EffectManager.HeightScale;
            heightSlider.Value = EffectManager.Height;
            redSlider.Value = EffectManager.FogColourRed;
            greenSlider.Value = EffectManager.FogColourGreen;
            blueSlider.Value = EffectManager.FogColourBlue;
        }

        protected override void DrawPane()
        {
            GUILayoutOption sliderWidth = MpsGui.HalfSlider;

            GUILayout.BeginHorizontal();
            distanceSlider.Draw(sliderWidth);
            densitySlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            heightScaleSlider.Draw(sliderWidth);
            heightSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            redSlider.Draw(sliderWidth);
            greenSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            blueSlider.Draw(sliderWidth);
        }
    }
}
