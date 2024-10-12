namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class DeskPropModel(DeskManager.ItemData itemData, string name = "") : IEquatable<DeskPropModel>, IPropModel
{
    private readonly DeskManager.ItemData itemData = itemData ?? throw new ArgumentNullException(nameof(itemData));

    private string name = string.IsNullOrEmpty(name) ? itemData.name : name;

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? itemData.name : value;
    }

    public string IconFilename =>
        string.Empty;

    public int ID =>
        itemData.id;

    public int CategoryID =>
        itemData.category_id;

    public string PrefabName =>
        itemData.prefab_name;

    public string AssetName =>
        itemData.asset_name;

    public bool Equals(IPropModel other) =>
        other is DeskPropModel model && Equals(model);

    public bool Equals(DeskPropModel other)
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
