using System.Collections.Concurrent;

using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class MenuPropRepository : IEnumerable<MenuFilePropModel>
{
    private readonly IMenuPropsConfiguration menuPropsConfiguration;
    private readonly IMenuFileCacheSerializer menuFileCacheSerializer;

    private Dictionary<MPN, IList<MenuFilePropModel>> props;

    public MenuPropRepository(
        IMenuPropsConfiguration menuPropsConfiguration, IMenuFileCacheSerializer menuFileCacheSerializer)
    {
        this.menuPropsConfiguration = menuPropsConfiguration ?? throw new ArgumentNullException(nameof(menuPropsConfiguration));
        this.menuFileCacheSerializer = menuFileCacheSerializer ?? throw new ArgumentNullException(nameof(menuFileCacheSerializer));

        Translation.ReloadTranslationEvent += OnReloadedTranslation;

        InitializeMenuFiles(menuPropsConfiguration);
    }

    public event EventHandler InitializingProps;

    public event EventHandler InitializedProps;

    public IEnumerable<MPN> CategoryMpn =>
        Props.Keys;

    public bool Busy =>
        menuPropsConfiguration.ModMenuPropsOnly
            ? ProcessingProps
            : !GameMain.Instance.MenuDataBase.JobFinished() || ProcessingProps;

    private Dictionary<MPN, IList<MenuFilePropModel>> Props =>
        Busy
            ? throw new MenuPropRepositoryBusyException()
            : props;

    private bool ProcessingProps { get; set; } = true;

    public IList<MenuFilePropModel> this[MPN category] =>
        Props[category];

    public bool TryGetPropList(MPN category, out IList<MenuFilePropModel> propList) =>
        Props.TryGetValue(category, out propList);

    public bool ContainsCategory(MPN category) =>
        Props.ContainsKey(category);

    public void Refresh()
    {
        if (Busy)
            throw new MenuPropRepositoryBusyException();

        InitializeMenuFiles(menuPropsConfiguration);
    }

    public IEnumerator<MenuFilePropModel> GetEnumerator() =>
        Props.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public MenuFilePropModel GetByID(string id) =>
        this.FirstOrDefault(model => string.Equals(model.ID, id, StringComparison.OrdinalIgnoreCase));

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

            if (!menuPropsConfiguration.ModMenuPropsOnly && !GameMain.Instance.MenuDataBase.JobFinished())
                while (!GameMain.Instance.MenuDataBase.JobFinished())
                    yield return wait;

            Task<Dictionary<MPN, IList<MenuFilePropModel>>>.Factory.StartNew(
                () => ProcessMenuFiles(menuPropsConfiguration, menuFileCacheSerializer))
                .ContinueWith(task =>
                {
                    props = task.Result;

                    ProcessingProps = false;

                    InitializedProps?.Invoke(this, EventArgs.Empty);
                });
        }

        static Dictionary<MPN, IList<MenuFilePropModel>> ProcessMenuFiles(
            IMenuPropsConfiguration menuPropsConfiguration,
            IMenuFileCacheSerializer menuFileCacheSerializer)
        {
            var validMpn = new HashSet<MPN>(
                SafeMpn.GetValues(
                    nameof(MPN.acchat),
                    nameof(MPN.headset),
                    nameof(MPN.wear),
                    nameof(MPN.skirt),
                    nameof(MPN.onepiece),
                    nameof(MPN.mizugi),
                    nameof(MPN.bra),
                    nameof(MPN.panz),
                    nameof(MPN.stkg),
                    nameof(MPN.shoes),
                    nameof(MPN.acckami),
                    nameof(MPN.megane),
                    nameof(MPN.acchead),
                    nameof(MPN.acchana),
                    nameof(MPN.accmimi),
                    nameof(MPN.glove),
                    nameof(MPN.acckubi),
                    nameof(MPN.acckubiwa),
                    nameof(MPN.acckamisub),
                    nameof(MPN.accnip),
                    nameof(MPN.accude),
                    nameof(MPN.accheso),
                    nameof(MPN.accashi),
                    nameof(MPN.accsenaka),
                    nameof(MPN.accshippo),
                    nameof(MPN.accxxx),
                    nameof(MPN.handitem),
                    nameof(MPN.kousoku_lower),
                    nameof(MPN.kousoku_upper)));

            var alwaysValidMpn = new HashSet<MPN>(
                SafeMpn.GetValues(nameof(MPN.handitem), nameof(MPN.kousoku_lower), nameof(MPN.kousoku_upper)));

            var menuFileCache = new ConcurrentDictionary<string, MenuFilePropModel>(menuFileCacheSerializer.Deserialize());
            var menuFileParser = new MenuFileParser();
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

                if (!menuFileCache.TryGetValue(menuFilename, out var menuFile))
                {
                    menuFile = menuFileParser.ParseMenuFile(menuFilename, true);

                    if (menuFile is null)
                        continue;

                    menuFileCache.TryAdd(menuFilename, menuFile);
                }

                if (menuFile.CategoryMpn == SafeMpn.GetValue(nameof(MPN.handitem)))
                    menuFile.Name = Translation.Get("propNames", menuFile.Filename);

                models.Add(menuFile);
            }

            Parallel.ForEach(
                GameUty.ModOnlysMenuFiles,
                new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 4) },
                menuFilename =>
                {
                    if (string.IsNullOrEmpty(menuFilename))
                        return;

                    if (!menuFileCache.TryGetValue(menuFilename, out var menuFile))
                    {
                        try
                        {
                            menuFile = menuFileParser.ParseMenuFile(menuFilename, false);
                        }
                        catch
                        {
                            Utility.LogDebug($"Could not parse {menuFilename}");

                            return;
                        }

                        if (menuFile is null)
                            return;

                        menuFileCache.TryAdd(menuFilename, menuFile);
                    }

                    if (!validMpn.Contains(menuFile.CategoryMpn))
                        return;

                    lock (lockObject)
                        models.Add(menuFile);
                });

            menuFileCacheSerializer.Serialize(menuFileCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            return models
                .GroupBy(model => model.CategoryMpn, model => model)
                .ToDictionary(group => group.Key, group => (IList<MenuFilePropModel>)group.ToList().AsReadOnly());
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
            foreach (var prop in this[SafeMpn.GetValue(nameof(MPN.handitem))])
                prop.Name = Translation.Get("propNames", prop.Filename);
        }
    }
}
