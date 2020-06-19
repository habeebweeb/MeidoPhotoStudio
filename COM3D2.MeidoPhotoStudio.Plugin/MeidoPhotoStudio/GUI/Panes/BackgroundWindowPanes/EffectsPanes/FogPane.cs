using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class FogPane : EffectPane<FogEffectManager>
    {
        protected override FogEffectManager EffectManager { get; set; }
        private Slider distanceSlider;
        private Slider densitySlider;
        private Slider heightScaleSlider;
        private Slider heightSlider;
        private Slider redSlider;
        private Slider greenSlider;
        private Slider blueSlider;

        public FogPane(EffectManager effectManager) : base(effectManager.FogEffectManager)
        {
            this.distanceSlider = new Slider(
                Translation.Get("effectFog", "distance"), 0f, 30f, FogEffectManager.InitialDistance
            );
            this.densitySlider = new Slider(
                Translation.Get("effectFog", "density"), 0f, 10f, FogEffectManager.InitialDensity
            );
            this.heightScaleSlider = new Slider(
                Translation.Get("effectFog", "strength"), -5f, 20f, FogEffectManager.InitialHeightScale
            );
            this.heightSlider = new Slider(
                Translation.Get("effectFog", "height"), -10f, 10f, FogEffectManager.InitialHeight
            );
            Color initialFogColour = FogEffectManager.InitialColour;
            this.redSlider = new Slider(Translation.Get("backgroundWIndow", "red"), 0f, 1f, initialFogColour.r);
            this.greenSlider = new Slider(Translation.Get("backgroundWIndow", "green"), 0f, 1f, initialFogColour.g);
            this.blueSlider = new Slider(Translation.Get("backgroundWIndow", "blue"), 0f, 1f, initialFogColour.b);
            this.distanceSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.Distance = this.distanceSlider.Value;
            };
            this.densitySlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.Density = this.densitySlider.Value;
            };
            this.heightScaleSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.HeightScale = this.heightScaleSlider.Value;
            };
            this.heightSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.Height = this.heightSlider.Value;
            };
            this.redSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.FogColourRed = this.redSlider.Value;
            };
            this.greenSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.FogColourGreen = this.greenSlider.Value;
            };
            this.blueSlider.ControlEvent += (s, a) =>
            {
                if (this.updating) return;
                this.EffectManager.FogColourBlue = this.blueSlider.Value;
            };
        }

        protected override void TranslatePane()
        {
            this.distanceSlider.Label = Translation.Get("effectFog", "distance");
            this.densitySlider.Label = Translation.Get("effectFog", "density");
            this.heightScaleSlider.Label = Translation.Get("effectFog", "strength");
            this.heightSlider.Label = Translation.Get("effectFog", "height");
            this.redSlider.Label = Translation.Get("backgroundWIndow", "red");
            this.greenSlider.Label = Translation.Get("backgroundWIndow", "green");
            this.blueSlider.Label = Translation.Get("backgroundWIndow", "blue");
        }

        protected override void UpdateControls()
        {
            this.distanceSlider.Value = EffectManager.Distance;
            this.densitySlider.Value = EffectManager.Density;
            this.heightScaleSlider.Value = EffectManager.HeightScale;
            this.heightSlider.Value = EffectManager.Height;
            this.redSlider.Value = EffectManager.FogColourRed;
            this.greenSlider.Value = EffectManager.FogColourGreen;
            this.blueSlider.Value = EffectManager.FogColourBlue;
        }

        protected override void DrawPane()
        {
            GUILayoutOption sliderWidth = MiscGUI.HalfSlider;

            GUILayout.BeginHorizontal();
            this.distanceSlider.Draw(sliderWidth);
            this.densitySlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.heightScaleSlider.Draw(sliderWidth);
            this.heightSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            redSlider.Draw(sliderWidth);
            greenSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            blueSlider.Draw(sliderWidth);
        }
    }
}
