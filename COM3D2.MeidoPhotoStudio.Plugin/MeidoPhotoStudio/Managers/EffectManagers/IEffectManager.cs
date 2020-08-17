namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal interface IEffectManager : IManager
    {
        bool Ready { get; }
        bool Active { get; }
        void SetEffectActive(bool active);
        void Reset();
    }
}
