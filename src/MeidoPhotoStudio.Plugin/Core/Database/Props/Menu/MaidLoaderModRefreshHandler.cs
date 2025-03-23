using COM3D2.MaidLoader;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

public class MaidLoaderModRefreshHandler : IModRefreshHandler
{
    public MaidLoaderModRefreshHandler() =>
        RefreshMod.Refreshed += OnMaidLoaderRefreshedMods;

    public event EventHandler<ModRefreshEventArgs> RefreshedMods;

    private void OnMaidLoaderRefreshedMods(object sender, RefreshMod.RefreshEventArgs e) =>
        RefreshedMods?.Invoke(this, new(e.NewMenus, e.DeletedMenus));
}
