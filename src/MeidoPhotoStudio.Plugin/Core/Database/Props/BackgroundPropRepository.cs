using MeidoPhotoStudio.Plugin.Core.Database.Background;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class BackgroundPropRepository(BackgroundRepository backgroundRepository) : IEnumerable<BackgroundPropModel>
{
    private readonly BackgroundRepository backgroundRepository = backgroundRepository
        ?? throw new ArgumentNullException(nameof(backgroundRepository));

    private Dictionary<BackgroundCategory, IList<BackgroundPropModel>> props;

    public IEnumerable<BackgroundCategory> Categories =>
        Props.Keys;

    private Dictionary<BackgroundCategory, IList<BackgroundPropModel>> Props =>
        props ??= Initialize(backgroundRepository);

    public IList<BackgroundPropModel> this[BackgroundCategory category] =>
        Props[category];

    public bool TryGetPropList(BackgroundCategory category, out IList<BackgroundPropModel> propList) =>
        Props.TryGetValue(category, out propList);

    public bool ContainsCategory(BackgroundCategory category) =>
        Props.ContainsKey(category);

    public void Refresh() =>
        props = Initialize(backgroundRepository);

    public IEnumerator<BackgroundPropModel> GetEnumerator() =>
        Props.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private static Dictionary<BackgroundCategory, IList<BackgroundPropModel>>
        Initialize(BackgroundRepository backgroundRepository)
    {
        backgroundRepository.Refresh();

        var models = new Dictionary<BackgroundCategory, IList<BackgroundPropModel>>();

        foreach (var category in backgroundRepository.Categories)
            models[category] = backgroundRepository[category]
                .Select(model => new BackgroundPropModel(model))
                .ToList()
                .AsReadOnly();

        return models;
    }
}