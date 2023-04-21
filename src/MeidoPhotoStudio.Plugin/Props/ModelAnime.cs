namespace MeidoPhotoStudio.Plugin;

public readonly struct ModelAnime
{
    public ModelAnime(TBody.SlotID slot, string animationName, bool loop = false)
    {
        Slot = slot;
        AnimationName = animationName;
        Loop = loop;
    }

    public TBody.SlotID Slot { get; }

    public string AnimationName { get; }

    public bool Loop { get; }
}
