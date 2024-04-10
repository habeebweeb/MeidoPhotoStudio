namespace MeidoPhotoStudio.Plugin;

public class TabSelectionEventArgs(Constants.Window tab) : EventArgs
{
    public Constants.Window Tab { get; } = tab;
}
