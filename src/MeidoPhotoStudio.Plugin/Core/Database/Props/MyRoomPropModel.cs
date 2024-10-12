using MyRoomCustom;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class MyRoomPropModel(PlacementData.Data data, string name = "") : IEquatable<MyRoomPropModel>, IPropModel
{
    private readonly PlacementData.Data data = data ?? throw new ArgumentNullException(nameof(data));

    private string name = string.IsNullOrEmpty(name) ? data.drawName : name;

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

    public bool Equals(IPropModel other) =>
        other is MyRoomPropModel model && Equals(model);

    public bool Equals(MyRoomPropModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return ID == other.ID && CategoryID == other.CategoryID;
    }

    public override int GetHashCode() =>
        (ID, CategoryID).GetHashCode();
}
