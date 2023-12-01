namespace MeidoPhotoStudio.Database.Props.Menu;

public readonly struct ModelAnimation
{
    public TBody.SlotID Slot { get; init; }

    public string AnimationName { get; init; }

    public bool Loop { get; init; }
}
