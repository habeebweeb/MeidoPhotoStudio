using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Database.Character;

public class GameBlendSetRepository : IEnumerable<GameBlendSetModel>
{
    private Dictionary<string, IList<GameBlendSetModel>> blendSets;

    public GameBlendSetRepository() =>
        Plugin.Translation.ReloadTranslationEvent += OnReloadedTranslation;

    public IEnumerable<string> Categories =>
        BlendSets.Keys;

    private Dictionary<string, IList<GameBlendSetModel>> BlendSets =>
        blendSets ??= Initialize();

    public IList<GameBlendSetModel> this[string category] =>
        BlendSets[category];

    public bool ContainsCategory(string category) =>
        BlendSets.ContainsKey(category);

    public IEnumerator<GameBlendSetModel> GetEnumerator() =>
        BlendSets.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public GameBlendSetModel GetByID(long id) =>
        this.FirstOrDefault(model => model.ID == id);

    private static Dictionary<string, IList<GameBlendSetModel>> Initialize()
    {
        PhotoFaceData.Create();

        Dictionary<string, List<GameBlendSetModel>> blendSets = [];

        foreach (var (category, faceDataList) in PhotoFaceData.category_list)
        {
            if (!blendSets.ContainsKey(category))
                blendSets[category] = [];

            foreach (var faceData in faceDataList)
                blendSets[category].Add(new(faceData, Plugin.Translation.Get("faceBlendPresetsDropdown", faceData.name)));
        }

        return blendSets.ToDictionary(
            kvp => kvp.Key,
            kvp => (IList<GameBlendSetModel>)kvp.Value.AsReadOnly());
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var blendSet in this)
            blendSet.Name = Plugin.Translation.Get("faceBlendPresetsDropdown", blendSet.BlendSetName);
    }
}
