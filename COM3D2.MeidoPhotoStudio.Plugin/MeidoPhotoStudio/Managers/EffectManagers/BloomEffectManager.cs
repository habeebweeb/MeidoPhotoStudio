using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BloomEffectManager : IEffectManager
    {
        private Bloom Bloom { get; set; }
        private float initialIntensity;
        private int initialBlurIterations;
        private Color initialThresholdColour;
        private Bloom.HDRBloomMode initialHDRBloomMode;
        public bool IsReady { get; private set; }
        public bool IsActive { get; private set; }
        private float intensity;
        public float Intensity
        {
            get => intensity;// m_gcBloom.GetValue();
            set => intensity = value;
        }
        private int blurIterations;
        public int BlurIterations
        {
            get => blurIterations;
            set => blurIterations = Bloom.bloomBlurIterations = value;
        }
        public float BloomThresholdColorRed
        {
            get => BloomThresholdColour.r;
            set
            {
                Color colour = Bloom.bloomThreshholdColor;
                BloomThresholdColour = new Color(value, colour.g, colour.b);
            }
        }
        public float BloomThresholdColorGreen
        {
            get => BloomThresholdColour.g;
            set
            {
                Color colour = Bloom.bloomThreshholdColor;
                BloomThresholdColour = new Color(colour.r, value, colour.b);
            }
        }
        public float BloomThresholdColorBlue
        {
            get => BloomThresholdColour.b;
            set
            {
                Color colour = Bloom.bloomThreshholdColor;
                BloomThresholdColour = new Color(colour.r, colour.g, value);
            }
        }
        private Color bloomThresholdColour;
        public Color BloomThresholdColour
        {
            get => bloomThresholdColour;
            set => bloomThresholdColour = Bloom.bloomThreshholdColor = value;
        }
        private bool HDRBloomMode;
        public bool BloomHDR
        {
            get => HDRBloomMode;
            set
            {
                Bloom.hdr = value ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;
                HDRBloomMode = value;
            }
        }

        public void Activate()
        {
            if (Bloom == null)
            {
                IsReady = true;
                Bloom = GameMain.Instance.MainCamera.GetOrAddComponent<Bloom>();
                initialIntensity = Intensity = Bloom.bloomIntensity;
                initialBlurIterations = BlurIterations = Bloom.bloomBlurIterations;
                initialThresholdColour = BloomThresholdColour = Bloom.bloomThreshholdColor;
                initialHDRBloomMode = Bloom.hdr;
                BloomHDR = initialHDRBloomMode == Bloom.HDRBloomMode.On;
            }
        }

        public void Deactivate()
        {
            Intensity = initialIntensity;
            BlurIterations = initialBlurIterations;
            BloomThresholdColour = initialThresholdColour;
            BloomHDR = initialHDRBloomMode == Bloom.HDRBloomMode.On;
            BloomHDR = false;
            Bloom.enabled = true;
            IsActive = false;
        }

        public void Reset()
        {
            Bloom.bloomIntensity = initialIntensity;
            Bloom.bloomBlurIterations = initialBlurIterations;
            Bloom.bloomThreshholdColor = initialThresholdColour;
            Bloom.hdr = initialHDRBloomMode;
        }

        public void SetEffectActive(bool active)
        {
            Bloom.enabled = active;
            IsActive = active;
            if (this.IsActive)
            {
                Bloom.bloomIntensity = Intensity;
                Bloom.bloomBlurIterations = BlurIterations;
                Bloom.bloomThreshholdColor = BloomThresholdColour;
                Bloom.hdr = BloomHDR ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;
            }
            else Reset();
        }

        public void Update()
        {
            if (IsActive)
            {
                // Fuck this stupid shit
                Bloom.enabled = true;
                Bloom.bloomIntensity = intensity;
            }
        }
    }
}
