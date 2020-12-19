using System.Reflection;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class BloomEffectManager : IEffectManager
    {
        public const string header = "EFFECT_BLOOM";
        private const float bloomDefIntensity = 5.7f;
        private static readonly CameraMain camera = GameMain.Instance.MainCamera;
        private Bloom Bloom { get; set; }
        // CMSystem's bloomValue;
        private static int backupBloomValue;
        private static readonly float backup_m_fBloomDefIntensity;
        private static readonly FieldInfo m_fBloomDefIntensity
            = Utility.GetFieldInfo<CameraMain>("m_fBloomDefIntensity");
        private static float BloomDefIntensity
        {
            set => m_fBloomDefIntensity.SetValue(camera, value);
            get => (float)m_fBloomDefIntensity.GetValue(camera);
        }
        private float initialIntensity;
        private int initialBlurIterations;
        private Color initialThresholdColour;
        private Bloom.HDRBloomMode initialHDRBloomMode;
        public bool Ready { get; private set; }
        public bool Active { get; private set; }
        private float bloomValue;
        public float BloomValue
        {
            get => bloomValue;
            set => GameMain.Instance.CMSystem.BloomValue = (int)(bloomValue = value);
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
        private bool bloomHdr;
        public bool BloomHDR
        {
            get => bloomHdr;
            set
            {
                Bloom.hdr = value ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;
                bloomHdr = value;
            }
        }

        static BloomEffectManager() => backup_m_fBloomDefIntensity = BloomDefIntensity;

        public void Activate()
        {
            if (Bloom == null)
            {
                Ready = true;
                Bloom = GameMain.Instance.MainCamera.GetComponent<Bloom>();
                initialIntensity = bloomValue = 50f;
                initialBlurIterations = blurIterations = Bloom.bloomBlurIterations;
                initialThresholdColour = bloomThresholdColour = Bloom.bloomThreshholdColor;
                initialHDRBloomMode = Bloom.hdr;
                bloomHdr = Bloom.hdr == Bloom.HDRBloomMode.On;

                backupBloomValue = GameMain.Instance.CMSystem.BloomValue;
            }
            SetEffectActive(false);
        }

        public void Deactivate()
        {
            BloomValue = initialIntensity;
            BlurIterations = initialBlurIterations;
            BloomThresholdColour = initialThresholdColour;
            BloomHDR = initialHDRBloomMode == Bloom.HDRBloomMode.On;
            BloomHDR = false;
            Active = false;

            BloomDefIntensity = backup_m_fBloomDefIntensity;
            GameMain.Instance.CMSystem.BloomValue = backupBloomValue;
        }

        public void Reset()
        {
            GameMain.Instance.CMSystem.BloomValue = backupBloomValue;
            Bloom.bloomBlurIterations = initialBlurIterations;
            Bloom.bloomThreshholdColor = initialThresholdColour;
            Bloom.hdr = initialHDRBloomMode;

            BloomDefIntensity = backup_m_fBloomDefIntensity;
        }

        public void SetEffectActive(bool active)
        {
            if (Active = active)
            {
                backupBloomValue = GameMain.Instance.CMSystem.BloomValue;
                GameMain.Instance.CMSystem.BloomValue = (int)BloomValue;
                Bloom.bloomBlurIterations = BlurIterations;
                Bloom.bloomThreshholdColor = BloomThresholdColour;
                Bloom.hdr = BloomHDR ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;

                BloomDefIntensity = bloomDefIntensity;
            }
            else Reset();
        }

        public void Update() { }
    }
}
