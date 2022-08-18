using System;

namespace MeidoPhotoStudio.Plugin;

public abstract class BasePane
{
    protected BaseWindow parent;
    protected bool updating;

    protected BasePane() =>
        Translation.ReloadTranslationEvent += OnReloadTranslation;

    // TODO: This does not work how I think it works. Probably just remove entirely.
    ~BasePane() =>
        Translation.ReloadTranslationEvent -= OnReloadTranslation;

    public virtual bool Visible { get; set; }

    public virtual bool Enabled { get; set; }

    public virtual void SetParent(BaseWindow window) =>
        parent = window;

    public virtual void UpdatePane()
    {
    }

    public virtual void Draw()
    {
    }

    public virtual void Activate()
    {
    }

    public virtual void Deactivate()
    {
    }

    protected virtual void ReloadTranslation()
    {
    }

    private void OnReloadTranslation(object sender, EventArgs args) =>
        ReloadTranslation();
}
