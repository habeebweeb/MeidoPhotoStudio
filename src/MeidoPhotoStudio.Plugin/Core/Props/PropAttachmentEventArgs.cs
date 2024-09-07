namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropAttachmentEventArgs(PropController prop, CharacterController character, AttachPoint attachPoint)
    : EventArgs
{
    public PropController Prop { get; } = prop ?? throw new ArgumentNullException(nameof(prop));

    public CharacterController Character { get; } = character ?? throw new ArgumentNullException(nameof(Character));

    public AttachPoint AttachPoint { get; } = attachPoint;
}
