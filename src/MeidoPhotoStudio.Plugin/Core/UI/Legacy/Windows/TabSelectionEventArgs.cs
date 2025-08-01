namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TabSelectionEventArgs(MainWindow.Tab tab, bool forceMainWindowVisible) : EventArgs
{
    public MainWindow.Tab Tab { get; } = tab;

    public bool ForceVisible { get; } = forceMainWindowVisible;
}
