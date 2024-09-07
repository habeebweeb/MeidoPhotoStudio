namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class DeskPropModel(DeskManager.ItemData itemData, string name = "") : IPropModel
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
}