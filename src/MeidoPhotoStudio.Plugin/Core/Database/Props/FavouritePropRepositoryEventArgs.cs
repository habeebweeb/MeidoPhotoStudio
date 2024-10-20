namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class FavouritePropRepositoryEventArgs(FavouritePropModel favouritePropModel) : EventArgs
{
    public FavouritePropModel FavouriteProp { get; } = favouritePropModel
        ?? throw new ArgumentNullException(nameof(favouritePropModel));
}
