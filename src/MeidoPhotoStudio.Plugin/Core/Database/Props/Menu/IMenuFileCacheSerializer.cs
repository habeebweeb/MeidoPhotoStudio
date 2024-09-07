namespace MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;

public interface IMenuFileCacheSerializer
{
    Dictionary<string, MenuFilePropModel> Deserialize();

    void Serialize(Dictionary<string, MenuFilePropModel> menuFileCache);
}
