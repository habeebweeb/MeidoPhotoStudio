namespace MeidoPhotoStudio.Plugin;

public class ProcStartEventArgs(Maid maid) : EventArgs
{
    public readonly Maid Maid = maid;
}
