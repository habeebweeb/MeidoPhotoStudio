using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class MenuPropRepository : IEnumerable<MenuFilePropModel>
{
    private readonly Translation translation;
    private readonly IMenuPropsConfiguration menuPropsConfiguration;
    private readonly IMenuFileCacheSerializer menuFileCacheSerializer;
    private readonly IModRefreshHandler modRefreshHandler;
    private readonly HashSet<string> newMenuFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> deletedMenuFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<MPN, ReadOnlyCollection<MenuFilePropModel>> readOnlyProps = [];

    private Dictionary<MPN, List<MenuFilePropModel>> props;

    public MenuPropRepository(
        Translation translation,
        IMenuPropsConfiguration menuPropsConfiguration,
        IMenuFileCacheSerializer menuFileCacheSerializer,
        IModRefreshHandler modRefreshHandler)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.menuPropsConfiguration = menuPropsConfiguration ?? throw new ArgumentNullException(nameof(menuPropsConfiguration));
        this.menuFileCacheSerializer = menuFileCacheSerializer ?? throw new ArgumentNullException(nameof(menuFileCacheSerializer));
        this.modRefreshHandler = modRefreshHandler ?? throw new ArgumentNullException(nameof(modRefreshHandler));

        this.translation.Initialized += OnReloadedTranslation;
        this.modRefreshHandler.RefreshedMods += OnModsRefreshed;

        InitializeMenuFiles(menuPropsConfiguration);
    }

    public event EventHandler InitializingProps;

    public event EventHandler InitializedProps;

    public event EventHandler<MenuPropRepositoryChangedEventArgs> ChangedProps;

    public IEnumerable<MPN> CategoryMpn =>
        Props.Keys;

    public bool Busy =>
        menuPropsConfiguration.ModMenuPropsOnly
            ? ProcessingProps
            : !GameMain.Instance.MenuDataBase.JobFinished() || ProcessingProps;

    private Dictionary<MPN, List<MenuFilePropModel>> Props =>
        Busy
            ? throw new MenuPropRepositoryBusyException()
            : props;

    private bool ProcessingProps { get; set; } = true;

    public IList<MenuFilePropModel> this[MPN category] =>
        readOnlyProps.TryGetValue(category, out var readOnlyPropList)
            ? readOnlyPropList
            : (IList<MenuFilePropModel>)(readOnlyProps[category] = Props[category].AsReadOnly());

    public bool TryGetPropList(MPN category, out IList<MenuFilePropModel> propList)
    {
        propList = [];

        if (Busy)
            return false;

        if (readOnlyProps.TryGetValue(category, out var readOnlyList))
        {
            propList = readOnlyList;

            return true;
        }

        if (Props.TryGetValue(category, out var list))
        {
            propList = readOnlyProps[category] = list.AsReadOnly();

            return true;
        }

        return false;
    }

    public bool ContainsCategory(MPN category) =>
        Props.ContainsKey(category);

    public IEnumerator<MenuFilePropModel> GetEnumerator() =>
        Props.Values.SelectMany(static list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public MenuFilePropModel GetByID(string id) =>
        this.FirstOrDefault(model => string.Equals(model.ID, id, StringComparison.OrdinalIgnoreCase));

    internal void Destroy() =>
        modRefreshHandler.RefreshedMods -= OnModsRefreshed;

    private void InitializeMenuFiles(IMenuPropsConfiguration menuPropsConfiguration)
    {
        ProcessingProps = true;

        InitializingProps?.Invoke(this, EventArgs.Empty);

        new CoroutineRunner(Process)
        {
            Name = "[MPS Menu File Processor]",
        }.Start();

        IEnumerator Process()
        {
            var wait = new WaitForSeconds(0.5f);

            while (!GameMain.Instance.MenuDataBase.JobFinished())
                yield return wait;

            var task = Task<Dictionary<MPN, List<MenuFilePropModel>>>.Factory
                .StartNew(() => ProcessMenuFiles(menuPropsConfiguration, menuFileCacheSerializer));

            while (!task.IsCompleted)
                yield return wait;

            if (task.IsFaulted && task.Exception is not null)
                Plugin.Logger.LogWarning($"Could not initialize menu props because:\n{task.Exception}");

            props = task.IsFaulted ? [] : task.Result ?? [];

            ProcessingProps = false;

            InitializedProps?.Invoke(this, EventArgs.Empty);
        }

        Dictionary<MPN, List<MenuFilePropModel>> ProcessMenuFiles(
            IMenuPropsConfiguration menuPropsConfiguration,
            IMenuFileCacheSerializer menuFileCacheSerializer)
        {
            var validMpn = new HashSet<MPN>([
                SafeMpn.acchat,
                SafeMpn.headset,
                SafeMpn.wear,
                SafeMpn.skirt,
                SafeMpn.onepiece,
                SafeMpn.mizugi,
                SafeMpn.bra,
                SafeMpn.panz,
                SafeMpn.stkg,
                SafeMpn.shoes,
                SafeMpn.acckami,
                SafeMpn.megane,
                SafeMpn.acchead,
                SafeMpn.acchana,
                SafeMpn.accmimi,
                SafeMpn.glove,
                SafeMpn.acckubi,
                SafeMpn.acckubiwa,
                SafeMpn.acckamisub,
                SafeMpn.accnip,
                SafeMpn.accude,
                SafeMpn.accheso,
                SafeMpn.accashi,
                SafeMpn.accsenaka,
                SafeMpn.accshippo,
                SafeMpn.accxxx,
                SafeMpn.handitem,
                SafeMpn.kousoku_lower,
                SafeMpn.kousoku_upper]);

            var alwaysValidMpn = new HashSet<MPN>([SafeMpn.handitem, SafeMpn.kousoku_lower, SafeMpn.kousoku_upper]);

            var menuFileCache = new ConcurrentDictionary<string, MenuFilePropModel>(menuFileCacheSerializer.Deserialize());
            var menuFileParser = new MenuFileParser();
            var menuFilesToProcess = new List<(string FileName, bool IsGame)>();
            var lockObject = new object();
            var models = new List<MenuFilePropModel>();

            foreach (var menuDatabase in GameMain.Instance.MenuDataBase)
            {
                if (menuDatabase.GetBoDelOnly())
                    continue;

                if (!validMpn.Contains(menuDatabase.GetMpn()))
                    continue;

                var menuFilename = menuDatabase.GetMenuFileName();

                if (menuFilename.Contains("_crc") || menuFilename.Contains("crc_") || menuFilename.Contains("_del"))
                    continue;

                if (menuPropsConfiguration.ModMenuPropsOnly && !alwaysValidMpn.Contains(menuDatabase.GetMpn()))
                    continue;

                menuFilesToProcess.Add((menuFilename, true));
            }

            Parallel.ForEach(
                GameUty.ModOnlysMenuFiles
                .Select(fileName => (FileName: fileName, IsGame: false))
                .Concat(menuFilesToProcess),
                new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, (int)Math.Ceiling(Environment.ProcessorCount * 0.75)) },
                menuFile =>
                {
                    if (string.IsNullOrEmpty(menuFile.FileName))
                        return;

                    if (!menuFileCache.TryGetValue(menuFile.FileName, out var model))
                    {
                        try
                        {
                            model = menuFileParser.ParseMenuFile(menuFile.FileName, menuFile.IsGame);
                        }
                        catch
                        {
                            Plugin.Logger.LogDebug($"Could not parse {menuFile.FileName}");

                            return;
                        }

                        if (model is null)
                            return;

                        menuFileCache.TryAdd(menuFile.FileName, model);
                    }

                    if (!validMpn.Contains(model.CategoryMpn))
                        return;

                    if (model.CategoryMpn == SafeMpn.handitem)
                        model.Name = translation["propNames", model.Filename];

                    lock (lockObject)
                        models.Add(model);
                });

            menuFileCacheSerializer.Serialize(menuFileCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            return models
                .GroupBy(model => model.CategoryMpn, model => model)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());
        }
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        InitializedProps += OnPropsInitialized;

        if (!Busy)
        {
            InitializedProps -= OnPropsInitialized;
            ApplyTranslation();
        }

        void OnPropsInitialized(object sender, EventArgs e)
        {
            InitializedProps -= OnPropsInitialized;
            ApplyTranslation();
        }

        void ApplyTranslation()
        {
            foreach (var prop in this[SafeMpn.handitem])
                prop.Name = translation["propNames", prop.Filename];
        }
    }

    private void OnModsRefreshed(object sender, ModRefreshEventArgs e)
    {
        if (Busy)
        {
            Plugin.Logger.LogDebug("Menu file prop repository is busy. Mods cannot be refreshed.");

            newMenuFiles.ExceptWith(e.DeletedMenuFiles);
            newMenuFiles.UnionWith(e.NewMenuFiles);
            deletedMenuFiles.ExceptWith(e.NewMenuFiles);
            deletedMenuFiles.UnionWith(e.DeletedMenuFiles);

            InitializedProps -= OnInitialized;
            InitializedProps += OnInitialized;

            return;
        }

        UpdateMods(e.NewMenuFiles, e.DeletedMenuFiles);

        void OnInitialized(object sender, EventArgs e)
        {
            InitializedProps -= OnInitialized;

            UpdateMods(newMenuFiles, deletedMenuFiles);
        }

        void UpdateMods(IEnumerable<string> newMenuFiles, IEnumerable<string> deletedMenuFiles)
        {
            if (Busy)
                return;

            if (!newMenuFiles.Any() && !deletedMenuFiles.Any())
                return;

            var addedProps = AddProps(newMenuFiles);
            var deletedProps = DeleteProps(deletedMenuFiles);

            if (addedProps.Count is 0 && deletedProps.Count is 0)
                return;

            ChangedProps?.Invoke(this, new(addedProps, deletedProps));

            List<MenuFilePropModel> AddProps(IEnumerable<string> menuFiles)
            {
                var validMpn = new HashSet<MPN>([
                    SafeMpn.acchat,
                    SafeMpn.headset,
                    SafeMpn.wear,
                    SafeMpn.skirt,
                    SafeMpn.onepiece,
                    SafeMpn.mizugi,
                    SafeMpn.bra,
                    SafeMpn.panz,
                    SafeMpn.stkg,
                    SafeMpn.shoes,
                    SafeMpn.acckami,
                    SafeMpn.megane,
                    SafeMpn.acchead,
                    SafeMpn.acchana,
                    SafeMpn.accmimi,
                    SafeMpn.glove,
                    SafeMpn.acckubi,
                    SafeMpn.acckubiwa,
                    SafeMpn.acckamisub,
                    SafeMpn.accnip,
                    SafeMpn.accude,
                    SafeMpn.accheso,
                    SafeMpn.accashi,
                    SafeMpn.accsenaka,
                    SafeMpn.accshippo,
                    SafeMpn.accxxx,
                    SafeMpn.handitem,
                    SafeMpn.kousoku_lower,
                    SafeMpn.kousoku_upper]);

                var parser = new MenuFileParser();
                var addedProps = new List<MenuFilePropModel>();

                foreach (var filename in newMenuFiles)
                {
                    if (string.IsNullOrEmpty(filename))
                        continue;

                    MenuFilePropModel model = null;

                    try
                    {
                        model = parser.ParseMenuFile(filename, false);
                    }
                    catch
                    {
                        Plugin.Logger.LogDebug($"Could not parse {filename}");

                        continue;
                    }

                    if (model is null)
                        continue;

                    if (!validMpn.Contains(model.CategoryMpn))
                        continue;

                    if (Props[model.CategoryMpn].Exists(prop => prop.Equals(model)))
                        continue;

                    Props[model.CategoryMpn].Add(model);
                    addedProps.Add(model);
                }

                return addedProps;
            }

            List<MenuFilePropModel> DeleteProps(IEnumerable<string> menuFiles)
            {
                var allMods = new Dictionary<string, MenuFilePropModel>(StringComparer.OrdinalIgnoreCase);
                var deletedProps = new List<MenuFilePropModel>();

                foreach (var prop in Props.Values.SelectMany(static props => props))
                {
                    if (allMods.ContainsKey(prop.Filename))
                        continue;

                    allMods[prop.Filename] = prop;
                }

                foreach (var filename in deletedMenuFiles)
                {
                    if (string.IsNullOrEmpty(filename))
                        continue;

                    if (!allMods.TryGetValue(filename, out var model))
                        continue;

                    Props[model.CategoryMpn].Remove(model);
                    deletedProps.Add(model);
                }

                return deletedProps;
            }
        }
    }
}
