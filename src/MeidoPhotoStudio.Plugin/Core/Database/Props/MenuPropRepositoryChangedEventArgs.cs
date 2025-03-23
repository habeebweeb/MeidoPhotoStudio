using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class MenuPropRepositoryChangedEventArgs(
    IEnumerable<MenuFilePropModel> addedMenuFiles,
    IEnumerable<MenuFilePropModel> deletedMenuFiles) : EventArgs
{
    public IEnumerable<MenuFilePropModel> AddedMenuFiles { get; } =
        addedMenuFiles ?? throw new ArgumentNullException(nameof(addedMenuFiles));

    public IEnumerable<MenuFilePropModel> DeletedMenuFiles { get; } =
        deletedMenuFiles ?? throw new ArgumentNullException(nameof(deletedMenuFiles));
}
