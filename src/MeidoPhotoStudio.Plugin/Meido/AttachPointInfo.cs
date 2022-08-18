namespace MeidoPhotoStudio.Plugin;

public readonly struct AttachPointInfo
{
    private static readonly AttachPointInfo EmptyValue = new(AttachPoint.None, string.Empty, -1);

    public AttachPointInfo(AttachPoint attachPoint, Meido meido)
    {
        AttachPoint = attachPoint;
        MaidGuid = meido.Maid.status.guid;
        MaidIndex = meido.Slot;
    }

    public AttachPointInfo(AttachPoint attachPoint, string maidGuid, int maidIndex)
    {
        AttachPoint = attachPoint;
        MaidGuid = maidGuid;
        MaidIndex = maidIndex;
    }

    public static ref readonly AttachPointInfo Empty =>
        ref EmptyValue;

    public AttachPoint AttachPoint { get; }

    public string MaidGuid { get; }

    public int MaidIndex { get; }
}
