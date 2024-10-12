using System.Collections.ObjectModel;

using MeidoPhotoStudio.Plugin.Core.Database.Background;
using Newtonsoft.Json;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class OtherPropRepository : IEnumerable<OtherPropModel>
{
    private readonly BackgroundRepository backgroundRepository;

    private Dictionary<string, IList<OtherPropModel>> props;

    public OtherPropRepository(BackgroundRepository backgroundRepository)
    {
        this.backgroundRepository = backgroundRepository ?? throw new ArgumentNullException(nameof(backgroundRepository));

        Translation.ReloadTranslationEvent += OnReloadedTranslation;
    }

    public IEnumerable<string> Categories =>
        Props.Keys;

    private Dictionary<string, IList<OtherPropModel>> Props =>
        props ??= Initialize(backgroundRepository);

    public IList<OtherPropModel> this[string category] =>
        Props[category];

    public bool TryGetPropList(string category, out IList<OtherPropModel> propList) =>
        Props.TryGetValue(category, out propList);

    public bool ContainsCategory(string category) =>
        Props.ContainsKey(category);

    public IEnumerator<OtherPropModel> GetEnumerator() =>
        Props.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public OtherPropModel GetByID(string id) =>
        this.FirstOrDefault(model => string.Equals(model.ID, id, StringComparison.OrdinalIgnoreCase));

    private static Dictionary<string, IList<OtherPropModel>> Initialize(BackgroundRepository backgroundRepository)
    {
        var models = new Dictionary<string, IList<OtherPropModel>>()
        {
            ["mob"] = GetMobProps(),
            ["game"] = GameProps(backgroundRepository),
        };

        var extend = GetPropExtend();

        if (extend.Any())
            models["extend"] = extend;

        return models;

        static ReadOnlyCollection<OtherPropModel> GetMobProps() =>
            new[]
            {
                "Mob_Man_Stand001", "Mob_Man_Stand002", "Mob_Man_Stand003", "Mob_Man_Sit001", "Mob_Man_Sit002",
                "Mob_Man_Sit003", "Mob_Girl_Stand001", "Mob_Girl_Stand002", "Mob_Girl_Stand003", "Mob_Girl_Sit001",
                "Mob_Girl_Sit002", "Mob_Girl_Sit003",
            }
            .Select(asset => new OtherPropModel(asset, Translation.Get("propNames", asset)))
            .ToList()
            .AsReadOnly();

        static ReadOnlyCollection<OtherPropModel> GameProps(BackgroundRepository backgroundRepository)
        {
            var otherProps = OtherPropsSet();

            return GameUty.FileSystem.GetList("bg", AFileSystemBase.ListType.AllFile)
                .Concat(GameUty.FileSystemOld.GetList("bg", AFileSystemBase.ListType.AllFile))
                .Where(path => Path.GetExtension(path) is ".asset_bg" && !path.StartsWith(@"bg\myroomcustomize"))
                .Select(Path.GetFileNameWithoutExtension)
                .Where(file => !file.EndsWith("_hit") && !file.EndsWith("_not_optimisation"))
                .Where(file => !otherProps.Contains(file))
                .Select(file => new OtherPropModel(file, Translation.Get("propNames", file)))
                .ToList()
                .AsReadOnly();

            HashSet<string> OtherPropsSet()
            {
                var set = new HashSet<string>(backgroundRepository.Select(background => background.AssetName), StringComparer.OrdinalIgnoreCase);

                PhotoBGObjectData.Create();

                set.UnionWith(PhotoBGObjectData.data
                    .Select(data => string.IsNullOrEmpty(data.create_asset_bundle_name)
                        ? data.create_prefab_name
                        : data.create_asset_bundle_name)
                    .Where(assetName => !string.IsNullOrEmpty(assetName)));

                set.UnionWith(DeskManager.item_detail_data_dic.Values
                    .Select(data => string.IsNullOrEmpty(data.asset_name)
                        ? data.prefab_name
                        : data.asset_name)
                    .Where(assetName => !string.IsNullOrEmpty(assetName)));

                return set;
            }
        }

        static ReadOnlyCollection<OtherPropModel> GetPropExtend()
        {
            try
            {
                var configPath = Path.Combine(BepInEx.Paths.ConfigPath, "MeidoPhotoStudio");
                var databasePath = Path.Combine(configPath, "Database");
                var propExtendFile = Path.Combine(databasePath, "extra_dogu.json");

                return JsonConvert.DeserializeObject<string[]>(File.ReadAllText(propExtendFile))
                    .Select(asset => new OtherPropModel(asset, Translation.Get("propNames", asset)))
                    .ToList()
                    .AsReadOnly();
            }
            catch (IOException e)
            {
                Utility.LogError($"Could not open extra prop database because {e.Message}");
            }
            catch (Exception e)
            {
                Utility.LogError($"Could not parse extra prop database because {e.Message}");
            }

            return new([]);
        }
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var prop in this)
            prop.Name = Translation.Get("propNames", prop.AssetName);
    }
}
