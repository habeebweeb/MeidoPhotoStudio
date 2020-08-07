namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EffectManager
    {
        public BloomEffectManager BloomEffectManager { get; }
        public DepthOfFieldEffectManager DepthOfFieldEffectManager { get; }
        public VignetteEffectManager VignetteEffectManager { get; }
        public FogEffectManager FogEffectManager { get; }

        public EffectManager()
        {
            BloomEffectManager = new BloomEffectManager();
            DepthOfFieldEffectManager = new DepthOfFieldEffectManager();
            VignetteEffectManager = new VignetteEffectManager();
            FogEffectManager = new FogEffectManager();
        }

        public void Activate()
        {
            BloomEffectManager.Activate();
            DepthOfFieldEffectManager.Activate();
            VignetteEffectManager.Activate();
            FogEffectManager.Activate();
        }

        public void Deactivate()
        {
            BloomEffectManager.Deactivate();
            DepthOfFieldEffectManager.Deactivate();
            VignetteEffectManager.Deactivate();
            FogEffectManager.Deactivate();
        }

        public void Update()
        {
            BloomEffectManager.Update();
        }
    }
}
