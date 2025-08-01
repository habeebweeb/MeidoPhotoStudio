namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TabSelectionController
{
    public event EventHandler<TabSelectionEventArgs> TabSelected;

    public void SelectTab(MainWindow.Tab tab, bool forceMainWindowVisible = false) =>
        TabSelected?.Invoke(this, new(tab, forceMainWindowVisible));
}
