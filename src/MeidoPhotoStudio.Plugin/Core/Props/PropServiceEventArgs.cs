namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropServiceEventArgs(PropController propController, int propIndex) : EventArgs
{
    public PropController PropController { get; } = propController
        ?? throw new ArgumentNullException(nameof(propController));

    public int PropIndex { get; } = propIndex;
}
