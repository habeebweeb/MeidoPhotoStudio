using System;

namespace MeidoPhotoStudio.Plugin;

public class MeidoUpdateEventArgs : EventArgs
{
    public MeidoUpdateEventArgs(int meidoIndex = -1, bool fromMaid = false, bool isBody = true)
    {
        SelectedMeido = meidoIndex;
        IsBody = isBody;
        FromMeido = fromMaid;
    }

    public static new MeidoUpdateEventArgs Empty { get; } = new(-1);

    public int SelectedMeido { get; }

    public bool IsBody { get; }

    public bool FromMeido { get; }

    public bool IsEmpty =>
        this == Empty || SelectedMeido is -1 && !FromMeido && IsBody;
}
