using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

namespace MeidoPhotoStudio.Database.Props.Menu;

public class MenuPropRepository : IEnumerable<MenuFilePropModel>
{
    private readonly IMenuPropsConfiguration menuPropsConfiguration;
    private readonly IMenuFileCacheSerializer menuFileCacheSerializer;

    private Dictionary<MPN, IList<MenuFilePropModel>> props;
    private Task<Dictionary<MPN, IList<MenuFilePropModel>>> propInitializationTask;

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

    private bool ProcessingProps =>
        props is null;

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
        props = null;

        InitializingProps?.Invoke(this, EventArgs.Empty);

        new CoroutineRunner(Process)
        {
            Name = "[MPS MenuDatabase Checker]",
        }.Start();

        IEnumerator Process()
        {
            var wait = new WaitForSeconds(0.5f);

            if (!menuPropsConfiguration.ModMenuPropsOnly && !GameMain.Instance.MenuDataBase.JobFinished())
                while (!GameMain.Instance.MenuDataBase.JobFinished())
                    yield return wait;

            propInitializationTask = Task<Dictionary<MPN, IList<MenuFilePropModel>>>.Factory.StartNew(
                () => ProcessMenuFiles(menuPropsConfiguration, menuFileCacheSerializer));

            while (!propInitializationTask.IsCompleted)
                yield return wait;

            props = propInitializationTask.Result;

            InitializedProps?.Invoke(this, EventArgs.Empty);
        }

        static Dictionary<MPN, IList<MenuFilePropModel>> ProcessMenuFiles(
            IMenuPropsConfiguration menuPropsConfiguration,
            IMenuFileCacheSerializer menuFileCacheSerializer)
        {
            var validMpn = new HashSet<MPN>()
            {
                MPN.acchat, MPN.headset, MPN.wear, MPN.skirt, MPN.onepiece, MPN.mizugi, MPN.bra, MPN.panz, MPN.stkg,
                MPN.shoes, MPN.acckami, MPN.megane, MPN.acchead, MPN.acchana, MPN.accmimi, MPN.glove, MPN.acckubi,
                MPN.acckubiwa, MPN.acckamisub, MPN.accnip, MPN.accude, MPN.accheso, MPN.accashi, MPN.accsenaka,
                MPN.accshippo, MPN.accxxx, MPN.handitem,
            };

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

                if (menuPropsConfiguration.ModMenuPropsOnly && menuDatabase.GetMpn() is not MPN.handitem)
                    continue;

                var menuFilename = menuDatabase.GetMenuFileName();

                if (!menuFileCache.TryGetValue(menuFilename, out var menuFile))
                {
                    menuFile = menuFileParser.ParseMenuFile(menuFilename, true);

                    if (menuFile is null)
                        continue;

                    menuFileCache.TryAdd(menuFilename, menuFile);
                }

                menuFile.Name = menuFile.CategoryMpn is MPN.handitem
                    ? Translation.Get("propNames", menuFile.Filename)
                    : menuFile.Filename;

                models.Add(menuFile);
            }

            Parallel.ForEach(
                GameUty.ModOnlysMenuFiles,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 4 },
                menuFilename =>
                {
                    if (!menuFileCache.TryGetValue(menuFilename, out var menuFile))
                    {
                        menuFile = menuFileParser.ParseMenuFile(menuFilename, false);

                        if (menuFile is null)
                            return;

                        menuFileCache.TryAdd(menuFilename, menuFile);
                    }

                    if (!validMpn.Contains(menuFile.CategoryMpn))
                        return;

                    menuFile.Name = menuFile.Filename;

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
            foreach (var prop in this[MPN.handitem])
                prop.Name = Translation.Get("propNames", prop.Filename);
        }
    }
}