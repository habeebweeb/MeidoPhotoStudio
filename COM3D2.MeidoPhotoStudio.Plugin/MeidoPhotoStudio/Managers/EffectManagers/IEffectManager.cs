namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal interface IEffectManager
    {
        bool Ready { get; }
        bool Active { get; }
        void Activate();
        void Deactivate();
        void SetEffectActive(bool active);
        void Reset();
        void Update();
    }
}
