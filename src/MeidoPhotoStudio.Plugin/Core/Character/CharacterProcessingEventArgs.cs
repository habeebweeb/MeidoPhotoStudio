namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterProcessingEventArgs(IEnumerable<MPN> changingSlots)
    : EventArgs
{
    public IEnumerable<MPN> ChangingSlots { get; } = changingSlots;
}
