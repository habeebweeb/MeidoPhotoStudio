namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropServiceEventArgs : EventArgs
{
    public PropServiceEventArgs(PropController propController, int propIndex)
    {
        PropController = propController ?? throw new ArgumentNullException(nameof(propController));
        PropIndex = propIndex;
    }

    public PropController PropController { get; }

    public int PropIndex { get; }
}
