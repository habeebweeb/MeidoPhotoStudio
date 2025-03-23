namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

public class ModRefreshEventArgs(IEnumerable<string> newMenuFiles, IEnumerable<string> deletedMenuFiles) : EventArgs
{
    public string[] NewMenuFiles { get; } =
        [.. newMenuFiles ?? throw new ArgumentNullException(nameof(newMenuFiles))];

    public string[] DeletedMenuFiles { get; } =
        [.. deletedMenuFiles ?? throw new ArgumentNullException(nameof(deletedMenuFiles))];
}
