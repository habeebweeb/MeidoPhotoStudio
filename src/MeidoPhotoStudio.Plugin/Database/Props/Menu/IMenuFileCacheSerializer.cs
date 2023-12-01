using System.Collections.Generic;

namespace MeidoPhotoStudio.Database.Props.Menu;

public interface IMenuFileCacheSerializer
{
    Dictionary<string, MenuFilePropModel> Deserialize();

    void Serialize(Dictionary<string, MenuFilePropModel> menuFileCache);
}
