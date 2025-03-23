namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

public interface IModRefreshHandler
{
    event EventHandler<ModRefreshEventArgs> RefreshedMods;
}
