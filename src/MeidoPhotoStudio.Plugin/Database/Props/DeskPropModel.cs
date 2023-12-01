namespace MeidoPhotoStudio.Database.Props;

public class DeskPropModel : IPropModel
{
    private readonly DeskManager.ItemData itemData;

    private string name;

    public DeskPropModel(DeskManager.ItemData itemData, string name = "")
    {
        this.itemData = itemData ?? throw new System.ArgumentNullException(nameof(itemData));
        this.name = string.IsNullOrEmpty(name) ? itemData.name : name;
    }

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
}
