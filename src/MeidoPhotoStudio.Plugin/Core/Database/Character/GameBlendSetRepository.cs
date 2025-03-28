using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Database.Character;

public class GameBlendSetRepository : IEnumerable<GameBlendSetModel>
{
    private readonly Translation translation;

    private Dictionary<string, IList<GameBlendSetModel>> blendSets;

    public GameBlendSetRepository(Translation translation)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.translation.Initialized += OnReloadedTranslation;
    }

    public IEnumerable<string> Categories =>
        BlendSets.Keys;

    private Dictionary<string, IList<GameBlendSetModel>> BlendSets =>
        blendSets ??= Initialize();

    public IList<GameBlendSetModel> this[string category] =>
        BlendSets[category];

    public bool ContainsCategory(string category) =>
        BlendSets.ContainsKey(category);

    public IEnumerator<GameBlendSetModel> GetEnumerator() =>
        BlendSets.Values.SelectMany(static list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public GameBlendSetModel GetByID(long id) =>
        this.FirstOrDefault(model => model.ID == id);

    private Dictionary<string, IList<GameBlendSetModel>> Initialize()
    {
        PhotoFaceData.Create();

        Dictionary<string, List<GameBlendSetModel>> blendSets = [];

        foreach (var (category, faceDataList) in PhotoFaceData.category_list)
        {
            if (!blendSets.ContainsKey(category))
                blendSets[category] = [];

            foreach (var faceData in faceDataList)
                blendSets[category].Add(new(faceData, translation["faceBlendPresetsDropdown", faceData.setting_name]));
        }

        return blendSets.ToDictionary(
            static kvp => kvp.Key,
            static kvp => (IList<GameBlendSetModel>)kvp.Value.AsReadOnly());
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var blendSet in this)
            blendSet.Name = translation["faceBlendPresetsDropdown", blendSet.BlendSetName];
    }
}
