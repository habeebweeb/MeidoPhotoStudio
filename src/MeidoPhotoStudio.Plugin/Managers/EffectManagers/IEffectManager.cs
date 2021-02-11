namespace MeidoPhotoStudio.Plugin
{
    public interface IEffectManager : IManager
    {
        bool Ready { get; }
        bool Active { get; }
        void SetEffectActive(bool active);
        void Reset();
    }
}
