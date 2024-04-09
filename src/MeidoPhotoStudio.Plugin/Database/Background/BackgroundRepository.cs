using System.Collections.ObjectModel;

using MeidoPhotoStudio.Plugin;
using MyRoomCustom;

namespace MeidoPhotoStudio.Database.Background;

public class BackgroundRepository : IEnumerable<BackgroundModel>
{
    private static ReadOnlyCollection<BackgroundModel> com3d2BackgroundsCache;
    private static ReadOnlyCollection<BackgroundModel> cm3d2BackgroundsCache;

    private Dictionary<BackgroundCategory, IList<BackgroundModel>> backgrounds;

    public BackgroundRepository() =>
        Translation.ReloadTranslationEvent += OnReloadedTranslation;

    public IEnumerable<BackgroundCategory> Categories =>
        Backgrounds.Keys;

    private Dictionary<BackgroundCategory, IList<BackgroundModel>> Backgrounds =>
        backgrounds is not null ? backgrounds : backgrounds = InitializeBackgrounds();

    public IList<BackgroundModel> this[BackgroundCategory category] =>
        Backgrounds[category];

    public IEnumerator<BackgroundModel> GetEnumerator() =>
        Backgrounds.Values.SelectMany(lists => lists).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Refresh() =>
        backgrounds = InitializeBackgrounds();

    public bool ContainsCategory(BackgroundCategory category) =>
        Backgrounds.ContainsKey(category);

    public BackgroundModel GetByID(string id)
    {
        foreach (var model in this)
        {
            if (string.Equals(id, model.ID, StringComparison.OrdinalIgnoreCase))
                return model;
        }

        return null;
    }

    private static CsvParser OpenCsvParser(string neiFile, AFileSystemBase fileSystem)
    {
        CsvParser csvParser = null;
        AFileBase aFileBase = null;

        try
        {
            if (!fileSystem.IsExistentFile(neiFile))
                return null;

            aFileBase = fileSystem.FileOpen(neiFile);
            csvParser = new CsvParser();

            if (csvParser.Open(aFileBase))
                return csvParser;
        }
        catch
        {
        }

        csvParser?.Dispose();
        aFileBase?.Dispose();

        return null;
    }

    private static Dictionary<BackgroundCategory, IList<BackgroundModel>> InitializeBackgrounds()
    {
        var backgrounds = new Dictionary<BackgroundCategory, IList<BackgroundModel>>
        {
            [BackgroundCategory.COM3D2] = GetCOM3D2Backgrounds(),
        };

        var cm3d2Backgrounds = GetCM3D2Backgrounds();

        if (cm3d2Backgrounds.Any())
            backgrounds[BackgroundCategory.CM3D2] = cm3d2Backgrounds;

        var myRoomCustomBackgrounds = GetMyRoomCustomBackgrounds();

        if (myRoomCustomBackgrounds.Any())
            backgrounds[BackgroundCategory.MyRoomCustom] = myRoomCustomBackgrounds;

        return backgrounds;

        static ReadOnlyCollection<BackgroundModel> GetCOM3D2Backgrounds()
        {
            if (com3d2BackgroundsCache is not null)
                return com3d2BackgroundsCache;

            PhotoBGData.Create();

            com3d2BackgroundsCache ??= PhotoBGData.data
                .Where(bgData => !string.IsNullOrEmpty(bgData.create_prefab_name))
                .Select(bgData => new BackgroundModel(
                    BackgroundCategory.COM3D2,
                    bgData.create_prefab_name,
                    Translation.Get("bgNames", bgData.create_prefab_name)))
                .ToList()
                .AsReadOnly();

            return com3d2BackgroundsCache;
        }

        static ReadOnlyCollection<BackgroundModel> GetCM3D2Backgrounds()
        {
            if (cm3d2BackgroundsCache is not null)
                return cm3d2BackgroundsCache;

            if (!GameUty.IsEnabledCompatibilityMode)
                return Enumerable.Empty<BackgroundModel>().ToList().AsReadOnly();

            using var csvParser = OpenCsvParser("phot_bg_list.nei", GameUty.FileSystemOld);

            var cm3d2Backgrounds = new List<BackgroundModel>();

            for (var cellY = 1; cellY < csvParser.max_cell_y; cellY++)
            {
                if (!csvParser.IsCellToExistData(3, cellY))
                    continue;

                var assetName = csvParser.GetCellAsString(3, cellY);

                cm3d2Backgrounds.Add(
                    new(BackgroundCategory.CM3D2, assetName, Translation.Get("bgNames", assetName)));
            }

            return cm3d2BackgroundsCache = cm3d2Backgrounds.AsReadOnly();
        }

        static ReadOnlyCollection<BackgroundModel> GetMyRoomCustomBackgrounds()
        {
            var creativeRoomData = CreativeRoomManager.GetSaveDataDic();

            return (creativeRoomData is null
                ? Enumerable.Empty<BackgroundModel>()
                : creativeRoomData.Select(data =>
                    new BackgroundModel(BackgroundCategory.MyRoomCustom, data.Key, data.Value)))
                    .ToList()
                    .AsReadOnly();
        }
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var background in this.Where(background => background.Category is not BackgroundCategory.MyRoomCustom))
            background.Name = Translation.Get("bgNames", background.AssetName);
    }
}
