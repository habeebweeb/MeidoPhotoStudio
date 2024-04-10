namespace MeidoPhotoStudio.Plugin.Core.Props;

public readonly record struct AttachPointInfo(AttachPoint AttachPoint, string MaidGuid, int MaidIndex)
{
    private static readonly AttachPointInfo EmptyValue = new(AttachPoint.None, string.Empty, -1);

    public AttachPointInfo(AttachPoint attachPoint, CharacterController character)
        : this(attachPoint, character.ID, character.Slot)
    {
    }

    public static ref readonly AttachPointInfo Empty =>
        ref EmptyValue;
}
