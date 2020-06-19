namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal interface IEffectManager
    {
        bool IsReady { get; }
        bool IsActive { get; }
        void Activate();
        void Deactivate();
        void SetEffectActive(bool active);
        void Reset();
        void Update();
    }
}
