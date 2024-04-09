using MyRoomCustom;

namespace MeidoPhotoStudio.Database.Props;

public class MyRoomPropModel : IPropModel
{
    private readonly PlacementData.Data data;

    private string name;

    public MyRoomPropModel(PlacementData.Data data, string name = "")
    {
        this.data = data ?? throw new ArgumentNullException(nameof(data));

        this.name = string.IsNullOrEmpty(name) ? data.drawName : name;
    }

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? data.drawName : value;
    }

    public string IconFilename =>
        data.thumbnailName;

    public int ID =>
        data.ID;

    public int CategoryID =>
        data.categoryID;

    public string AssetName =>
        string.IsNullOrEmpty(data.assetName) ? data.resourceName : data.assetName;
}
