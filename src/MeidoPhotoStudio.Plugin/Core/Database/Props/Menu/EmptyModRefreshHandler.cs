namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

internal sealed class EmptyModRefreshHandler : IModRefreshHandler
{
    public event EventHandler<ModRefreshEventArgs> RefreshedMods
    {
        add { }
        remove { }
    }
}
