using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DepthOfFieldPane : EffectPane<DepthOfFieldEffectManager>
    {
        protected override DepthOfFieldEffectManager EffectManager { get; set; }
        private Slider focalLengthSlider;
        private Slider focalSizeSlider;
        private Slider apertureSlider;
        private Slider blurSlider;
        private Toggle thicknessToggle;

        public DepthOfFieldPane(EffectManager effectManager) : base(effectManager.DepthOfFieldEffectManager)
        {
            this.focalLengthSlider = new Slider(Translation.Get("effectDof", "focalLength"), 0f, 10f);
            this.focalSizeSlider = new Slider(Translation.Get("effectDof", "focalArea"), 0f, 2f);
            this.apertureSlider = new Slider(Translation.Get("effectDof", "aperture"), 0f, 60f);
            this.blurSlider = new Slider(Translation.Get("effectDof", "blur"), 0f, 10f);
            this.thicknessToggle = new Toggle(Translation.Get("effectDof", "thicknessToggle"));
            this.focalLengthSlider.ControlEvent += (s, a) =>
            {
                this.EffectManager.FocalLength = this.focalLengthSlider.Value;
            };
            this.focalSizeSlider.ControlEvent += (s, a) =>
            {
                this.EffectManager.FocalSize = this.focalSizeSlider.Value;
            };
            this.apertureSlider.ControlEvent += (s, a) =>
            {
                this.EffectManager.Aperture = this.apertureSlider.Value;
            };
            this.blurSlider.ControlEvent += (s, a) =>
            {
                this.EffectManager.MaxBlurSize = this.blurSlider.Value;
            };
            this.thicknessToggle.ControlEvent += (s, a) =>
            {
                this.EffectManager.VisualizeFocus = this.thicknessToggle.Value;
            };
        }

        protected override void TranslatePane()
        {
            this.focalLengthSlider.Label = Translation.Get("effectDof", "focalLength");
            this.focalSizeSlider.Label = Translation.Get("effectDof", "focalArea");
            this.apertureSlider.Label = Translation.Get("effectDof", "aperture");
            this.blurSlider.Label = Translation.Get("effectDof", "blur");
            this.thicknessToggle.Label = Translation.Get("effectDof", "thicknessToggle");
        }

        protected override void UpdateControls()
        {
            this.focalLengthSlider.Value = this.EffectManager.FocalLength;
            this.focalSizeSlider.Value = this.EffectManager.FocalSize;
            this.apertureSlider.Value = this.EffectManager.Aperture;
            this.blurSlider.Value = this.EffectManager.MaxBlurSize;
            this.thicknessToggle.Value = this.EffectManager.VisualizeFocus;
        }

        protected override void DrawPane()
        {
            this.focalLengthSlider.Draw();

            GUILayoutOption sliderWidth = MiscGUI.HalfSlider;

            GUILayout.BeginHorizontal();
            this.focalSizeSlider.Draw(sliderWidth);
            this.apertureSlider.Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.blurSlider.Draw(sliderWidth);
            GUILayout.FlexibleSpace();
            this.thicknessToggle.Draw();
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
    }
}
