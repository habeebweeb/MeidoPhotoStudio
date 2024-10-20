namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public interface IFavouritePropListSerializer
{
    IEnumerable<FavouritePropModel> Deserialize();

    void Serialize(IEnumerable<FavouritePropModel> favouriteProps);
}
