namespace MeidoPhotoStudio.Plugin.Core.Patchers;

public class ProcStartEventArgs(Maid maid) : EventArgs
{
    public readonly Maid Maid = maid;
}
