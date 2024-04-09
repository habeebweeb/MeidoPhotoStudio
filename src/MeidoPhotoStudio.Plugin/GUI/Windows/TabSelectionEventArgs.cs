namespace MeidoPhotoStudio.Plugin;

public class TabSelectionEventArgs : EventArgs
{
    public TabSelectionEventArgs(Constants.Window tab) =>
        Tab = tab;

    public Constants.Window Tab { get; }
}
