using System.ComponentModel;

namespace MeidoPhotoStudio.Plugin.Core.Effects;

public abstract class EffectControllerBase : INotifyPropertyChanged
{
    private bool active;

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual bool Active
    {
        get => active;
        set
        {
            active = value;

            RaisePropertyChanged(nameof(Active));
        }
    }

    public abstract void Reset();

    internal virtual void Activate() =>
        Active = false;

    internal virtual void Deactivate() =>
        Active = false;

    protected void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
