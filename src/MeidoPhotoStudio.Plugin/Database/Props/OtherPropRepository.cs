using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin;
using Newtonsoft.Json;

namespace MeidoPhotoStudio.Database.Props;

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
        this.FirstOrDefault(model => string.Equals(model.ID, id));

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
            var backgroundSet = new HashSet<string>(backgroundRepository.Select(background => background.AssetName));

            return GameUty.FileSystem.GetList("bg", AFileSystemBase.ListType.AllFile)
                .Concat(GameUty.FileSystemOld.GetList("bg", AFileSystemBase.ListType.AllFile))
                .Where(path => Path.GetExtension(path) is ".asset_bg" && !path.StartsWith(@"bg\myroomcustomize"))
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Where(file => !file.EndsWith("_hit") && !file.EndsWith("_not_optimisation"))
                .Where(file => !backgroundSet.Contains(file))
                .Select(file => new OtherPropModel(file, Translation.Get("propNames", file)))
                .ToList()
                .AsReadOnly();
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

            return Enumerable.Empty<OtherPropModel>().ToList().AsReadOnly();
        }
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var prop in this)
            prop.Name = Translation.Get("propNames", prop.AssetName);
    }
}
