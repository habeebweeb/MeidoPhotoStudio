namespace MeidoPhotoStudio.Plugin.Core.Effects;

public abstract class EffectControllerBase
{
    public virtual bool Active { get; set; }

    public abstract void Reset();

    internal virtual void Activate() =>
        Active = false;

    internal virtual void Deactivate() =>
        Active = false;
}
