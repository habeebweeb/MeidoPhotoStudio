using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class VignetteEffectManager : IEffectManager
    {
        private Vignetting Vignette { get; set; }
        private float initialIntensity;
        private float initialBlur;
        private float initialBlurSpread;
        private float initialChromaticAberration;
        public bool IsReady { get; private set; }
        public bool IsActive { get; private set; }
        private float intensity;
        public float Intensity
        {
            get => intensity;
            set => intensity = Vignette.intensity = value;
        }
        private float blur;
        public float Blur
        {
            get => blur;
            set => blur = Vignette.blur = value;
        }
        private float blurSpread;
        public float BlurSpread
        {
            get => blurSpread;
            set => blurSpread = Vignette.blurSpread = value;
        }
        private float chromaticAberration;
        public float ChromaticAberration
        {
            get => chromaticAberration;
            set => chromaticAberration = Vignette.chromaticAberration = value;
        }

        public void Activate()
        {
            if (Vignette == null)
            {
                IsReady = true;
                Vignette = GameMain.Instance.MainCamera.GetOrAddComponent<Vignetting>();
                Vignette.mode = Vignetting.AberrationMode.Simple;

                initialIntensity = Vignette.intensity;
                initialBlur = Vignette.blur;
                initialBlurSpread = Vignette.blurSpread;
                initialChromaticAberration = Vignette.chromaticAberration;
            }
        }

        public void Deactivate()
        {
            Intensity = initialIntensity;
            Blur = initialBlur;
            BlurSpread = initialBlurSpread;
            ChromaticAberration = initialChromaticAberration;
            Vignette.enabled = false;
            IsActive = false;
        }

        public void Reset()
        {
            Vignette.intensity = initialIntensity;
            Vignette.blur = initialBlur;
            Vignette.blurSpread = initialBlurSpread;
            Vignette.chromaticAberration = initialChromaticAberration;
        }

        public void SetEffectActive(bool active)
        {
            Vignette.enabled = active;
            IsActive = active;
            if (this.IsActive)
            {
                Vignette.intensity = Intensity;
                Vignette.blur = Blur;
                Vignette.blurSpread = BlurSpread;
                Vignette.chromaticAberration = ChromaticAberration;
            }
            else Reset();
        }

        public void Update() { }
    }
}
