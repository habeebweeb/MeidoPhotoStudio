using System;

namespace MeidoPhotoStudio.Plugin;

public class ProcStartEventArgs : EventArgs
{
    public readonly Maid Maid;

    public ProcStartEventArgs(Maid maid) =>
        Maid = maid;
}
