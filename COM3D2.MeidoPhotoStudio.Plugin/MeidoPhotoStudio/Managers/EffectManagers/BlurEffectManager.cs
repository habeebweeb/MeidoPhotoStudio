namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class BlurEffectManager : IEffectManager
    {
        public const string header = "EFFECT_BLUR";
        private Blur Blur { get; set; }
        public bool Ready { get; private set; }
        public bool Active { get; private set; }
        private float initialBlurSize;
        private int initialBlurIterations;
        private int initialDownsample;
        private float blurSize;
        public float BlurSize
        {
            get => blurSize;
            set
            {
                blurSize = value;
                Blur.blurSize = blurSize / 10f;
                if (blurSize >= 3f)
                {
                    Blur.blurSize -= 0.3f;
                    Blur.blurIterations = 1;
                    Blur.downsample = 1;
                }
                else
                {
                    Blur.blurIterations = 0;
                    Blur.downsample = 0;
                }
            }
        }

        public void Activate()
        {
            if (Blur == null)
            {
                Ready = true;
                Blur = GameMain.Instance.MainCamera.GetComponent<Blur>();
                initialBlurSize = Blur.blurSize;
                initialBlurIterations = Blur.blurIterations;
                initialDownsample = Blur.downsample;
            }
            SetEffectActive(false);
        }

        public void Deactivate()
        {
            BlurSize = 0f;
            Reset();
            Blur.enabled = false;
            Active = false;
        }

        public void SetEffectActive(bool active)
        {
            if (Blur.enabled = Active = active) BlurSize = BlurSize;
            else Reset();
        }

        public void Reset()
        {
            Blur.blurSize = initialBlurSize;
            Blur.blurIterations = initialBlurIterations;
            Blur.downsample = initialDownsample;
        }

        public void Update() { }
    }
}
