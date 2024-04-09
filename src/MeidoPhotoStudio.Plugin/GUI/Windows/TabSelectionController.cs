namespace MeidoPhotoStudio.Plugin;

public class TabSelectionController
{
    public event EventHandler<TabSelectionEventArgs> TabSelected;

    public void SelectTab(Constants.Window tab) =>
        TabSelected?.Invoke(this, new(tab));
}
