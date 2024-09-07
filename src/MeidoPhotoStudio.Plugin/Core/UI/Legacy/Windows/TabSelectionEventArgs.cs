namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TabSelectionEventArgs(Constants.Window tab) : EventArgs
{
    public Constants.Window Tab { get; } = tab;
}
