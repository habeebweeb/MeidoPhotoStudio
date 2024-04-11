namespace MeidoPhotoStudio.Database.Props;

public class MyRoomPropRepository : IEnumerable<MyRoomPropModel>
{
    private Dictionary<int, IList<MyRoomPropModel>> props;

    public MyRoomPropRepository() =>
        Plugin.Translation.ReloadTranslationEvent += OnReloadedTranslation;

    public IEnumerable<int> CategoryIDs =>
        Props.Keys;

    private Dictionary<int, IList<MyRoomPropModel>> Props =>
        props ??= Initialize();

    public IList<MyRoomPropModel> this[int categoryID] =>
        Props[categoryID];

    public bool TryGetPropList(int categoryID, out IList<MyRoomPropModel> propList) =>
        Props.TryGetValue(categoryID, out propList);

    public bool ContainsCategory(int categoryID) =>
        Props.ContainsKey(categoryID);

    public IEnumerator<MyRoomPropModel> GetEnumerator() =>
        Props.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public MyRoomPropModel GetByID(int id) =>
        this.FirstOrDefault(model => model.ID == id);

    private static Dictionary<int, IList<MyRoomPropModel>> Initialize()
    {
        var models = new Dictionary<int, List<MyRoomPropModel>>();

        foreach (var data in MyRoomCustom.PlacementData.GetAllDatas(false))
        {
            var assetName = string.IsNullOrEmpty(data.resourceName) ? data.assetName : data.resourceName;
            var model = new MyRoomPropModel(data, Plugin.Translation.Get("myRoomPropNames", assetName));

            if (!models.ContainsKey(data.categoryID))
                models[data.categoryID] = [];

            models[model.CategoryID].Add(model);
        }

        return models.ToDictionary(kvp => kvp.Key, kvp => (IList<MyRoomPropModel>)kvp.Value.AsReadOnly());
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var prop in this)
            prop.Name = Plugin.Translation.Get("myRoomPropNames", prop.AssetName);
    }
}
