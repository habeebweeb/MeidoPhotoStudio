using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using MyRoomCustom;
using Newtonsoft.Json;
using UnityEngine;
using wf;

using static MeidoPhotoStudio.Plugin.MenuFileUtility;

namespace MeidoPhotoStudio.Plugin;

public static class Constants
{
    public const string CustomPoseDirectory = "Custom Poses";
    public const string CustomHandDirectory = "Hand Presets";
    public const string CustomFaceDirectory = "Face Presets";
    public const string SceneDirectory = "Scenes";
    public const string KankyoDirectory = "Environments";
    public const string ConfigDirectory = "MeidoPhotoStudio";
    public const string TranslationDirectory = "Translations";
    public const string DatabaseDirectory = "Database";

    public static readonly string CustomPosePath;
    public static readonly string CustomHandPath;
    public static readonly string CustomFacePath;
    public static readonly string ScenesPath;
    public static readonly string KankyoPath;
    public static readonly string ConfigPath;
    public static readonly string DatabasePath;

    // TODO: Some of these IDs aren't used to use them or drop them.
    public static readonly int MainWindowID = 765;
    public static readonly int MessageWindowID = 961;
    public static readonly int SceneManagerWindowID = 876;
    public static readonly int SceneManagerModalID = 283;
    public static readonly int DropdownWindowID = 777;

    public static readonly List<string> PoseGroupList = new();
    public static readonly Dictionary<string, List<string>> PoseDict = new();
    public static readonly List<string> CustomPoseGroupList = new();
    public static readonly Dictionary<string, List<string>> CustomPoseDict = new();
    public static readonly List<string> CustomHandGroupList = new();
    public static readonly Dictionary<string, List<string>> CustomHandDict = new();
    public static readonly List<string> FaceGroupList = new();
    public static readonly Dictionary<string, List<string>> FaceDict = new();
    public static readonly List<string> CustomFaceGroupList = new();
    public static readonly Dictionary<string, List<string>> CustomFaceDict = new();
    public static readonly List<string> BGList = new();
    public static readonly List<KeyValuePair<string, string>> MyRoomCustomBGList = new();
    public static readonly List<string> DoguCategories = new();
    public static readonly Dictionary<string, List<string>> DoguDict = new();
    public static readonly List<string> MyRoomPropCategories = new();
    public static readonly Dictionary<string, List<MyRoomItem>> MyRoomPropDict = new();
    public static readonly Dictionary<string, List<ModItem>> ModPropDict =
        new(StringComparer.InvariantCultureIgnoreCase);

    public static readonly List<string> SceneDirectoryList = new();
    public static readonly List<string> KankyoDirectoryList = new();
    public static readonly List<MpnAttachProp> MpnAttachPropList = new();
    public static readonly Dictionary<DoguCategory, string> CustomDoguCategories =
        new()
        {
            [DoguCategory.Other] = "other",
            [DoguCategory.Mob] = "mob",
            [DoguCategory.Desk] = "desk",
            [DoguCategory.HandItem] = "handItem",
            [DoguCategory.BGSmall] = "bgSmall",
        };

    private static bool beginHandItemInit;
    private static bool beginMpnAttachInit;

    static Constants()
    {
        ConfigPath = Path.Combine(BepInEx.Paths.ConfigPath, ConfigDirectory);

        var presetPath = Path.Combine(ConfigPath, "Presets");

        CustomPosePath = Path.Combine(presetPath, CustomPoseDirectory);
        CustomHandPath = Path.Combine(presetPath, CustomHandDirectory);
        CustomFacePath = Path.Combine(presetPath, CustomFaceDirectory);
        ScenesPath = Path.Combine(ConfigPath, SceneDirectory);
        KankyoPath = Path.Combine(ConfigPath, KankyoDirectory);
        DatabasePath = Path.Combine(ConfigPath, DatabaseDirectory);

        var directories =
            new[] { CustomPosePath, CustomHandPath, ScenesPath, KankyoPath, ConfigPath, CustomFacePath, DatabasePath };

        foreach (var directory in directories)
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
    }

    public static event EventHandler<MenuFilesEventArgs> MenuFilesChange;

    public static event EventHandler<PresetChangeEventArgs> CustomPoseChange;

    public static event EventHandler<PresetChangeEventArgs> CustomHandChange;

    public static event EventHandler<PresetChangeEventArgs> CustomFaceChange;

    public enum Window
    {
        Call,
        Pose,
        Face,
        BG,
        BG2,
        Main,
        Message,
        Save,
        SaveModal,
        Settings,
    }

    public enum Scene
    {
        Daily = 3,
        Edit = 5,
    }

    public enum DoguCategory
    {
        Other,
        Mob,
        Desk,
        HandItem,
        BGSmall,
    }

    public static int MyRoomCustomBGIndex { get; private set; } = -1;

    public static bool HandItemsInitialized { get; private set; }

    public static bool MpnAttachInitialized { get; private set; }

    public static bool MenuFilesInitialized { get; private set; }

    public static void Initialize()
    {
        InitializeSceneDirectories();
        InitializeKankyoDirectories();
        InitializePoses();
        InitializeHandPresets();
        InitializeFaceBlends();
        InitializeBGs();
        InitializeDogu();
        InitializeMyRoomProps();
        InitializeMpnAttachProps();
    }

    public static void AddFacePreset(Dictionary<string, float> faceData, string filename, string directory)
    {
        filename = Utility.SanitizePathPortion(filename);
        directory = Utility.SanitizePathPortion(directory);

        if (string.IsNullOrEmpty(filename))
            filename = "face_preset";

        if (directory.Equals(CustomFaceDirectory, StringComparison.InvariantCultureIgnoreCase))
            directory = string.Empty;

        directory = Path.Combine(CustomFacePath, directory);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, filename);

        if (File.Exists($"{fullPath}.xml"))
            fullPath += $"_{Utility.Timestamp}";

        fullPath = Path.GetFullPath($"{fullPath}.xml");

        if (!fullPath.StartsWith(CustomFacePath))
        {
            Utility.LogError($"Could not save face preset! Path is invalid: '{fullPath}'");

            return;
        }

        var rootElement = new XElement("FaceData");

        foreach (var kvp in faceData)
            rootElement.Add(new XElement("elm", kvp.Value.ToString("G9"), new XAttribute("name", kvp.Key)));

        var fullDocument = new XDocument(
            new XDeclaration("1.0", "utf-8", "true"),
            new XComment("MeidoPhotoStudio Face Preset"),
            rootElement);

        fullDocument.Save(fullPath);

        var fileInfo = new FileInfo(fullPath);
        var category = fileInfo.Directory.Name;
        var faceGroup = CustomFaceGroupList.Find(
            group => string.Equals(category, group, StringComparison.InvariantCultureIgnoreCase));

        if (string.IsNullOrEmpty(faceGroup))
        {
            CustomFaceGroupList.Add(category);
            CustomFaceDict[category] = new();
            CustomFaceGroupList.Sort((a, b) => KeepAtTop(a, b, CustomFaceDirectory));
        }
        else
        {
            category = faceGroup;
        }

        CustomFaceDict[category].Add(fullPath);
        CustomFaceDict[category].Sort(WindowsLogicalComparer.StrCmpLogicalW);

        CustomFaceChange?.Invoke(null, new(fullPath, category));
    }

    public static void AddPose(byte[] anmBinary, string filename, string directory)
    {
        // TODO: Consider writing a file system monitor
        filename = Utility.SanitizePathPortion(filename);
        directory = Utility.SanitizePathPortion(directory);

        if (string.IsNullOrEmpty(filename))
            filename = "custom_pose";

        if (directory.Equals(CustomPoseDirectory, StringComparison.InvariantCultureIgnoreCase))
            directory = string.Empty;

        directory = Path.Combine(CustomPosePath, directory);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, filename);

        if (File.Exists($"{fullPath}.anm"))
            fullPath += $"_{Utility.Timestamp}";

        fullPath = Path.GetFullPath($"{fullPath}.anm");

        if (!fullPath.StartsWith(CustomPosePath))
        {
            Utility.LogError($"Could not save pose! Path is invalid: '{fullPath}'");

            return;
        }

        File.WriteAllBytes(fullPath, anmBinary);

        var fileInfo = new FileInfo(fullPath);

        var category = fileInfo.Directory.Name;
        var poseGroup = CustomPoseGroupList.Find(
            group => string.Equals(category, group, StringComparison.InvariantCultureIgnoreCase));

        if (string.IsNullOrEmpty(poseGroup))
        {
            CustomPoseGroupList.Add(category);
            CustomPoseDict[category] = new();
            CustomPoseGroupList.Sort((a, b) => KeepAtTop(a, b, CustomPoseDirectory));
        }
        else
        {
            category = poseGroup;
        }

        CustomPoseDict[category].Add(fullPath);
        CustomPoseDict[category].Sort(WindowsLogicalComparer.StrCmpLogicalW);

        CustomPoseChange?.Invoke(null, new(fullPath, category));
    }

    public static void AddHand(byte[] handBinary, bool right, string filename, string directory)
    {
        filename = Utility.SanitizePathPortion(filename);
        directory = Utility.SanitizePathPortion(directory);

        if (string.IsNullOrEmpty(filename))
            filename = "custom_hand";

        if (directory.Equals(CustomHandDirectory, StringComparison.InvariantCultureIgnoreCase))
            directory = string.Empty;

        directory = Path.Combine(CustomHandPath, directory);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, filename);

        if (File.Exists($"{fullPath}.xml"))
            fullPath += $"_{Utility.Timestamp}";

        fullPath = Path.GetFullPath($"{fullPath}.xml");

        if (!fullPath.StartsWith(CustomHandPath))
        {
            Utility.LogError($"Could not save hand! Path is invalid: '{fullPath}'");

            return;
        }

        // TODO: This does not actually do what I think it does.
        var gameVersion = Misc.GAME_VERSION; // get game version from user's Assembly-CSharp
        var finalXml = new XDocument(
            new XDeclaration("1.0", "utf-8", "true"),
            new XComment("CM3D2 FingerData"),
            new XElement(
                "FingerData",
                new XElement("GameVersion", gameVersion),
                new XElement("RightData", right),
                new XElement("BinaryData", Convert.ToBase64String(handBinary))));

        finalXml.Save(fullPath);

        var fileInfo = new FileInfo(fullPath);
        var category = fileInfo.Directory.Name;
        var handGroup = CustomHandGroupList.Find(
            group => string.Equals(category, group, StringComparison.InvariantCultureIgnoreCase));

        if (string.IsNullOrEmpty(handGroup))
        {
            CustomHandGroupList.Add(category);
            CustomHandDict[category] = new();
            CustomHandGroupList.Sort((a, b) => KeepAtTop(a, b, CustomHandDirectory));
        }
        else
        {
            category = handGroup;
        }

        CustomHandDict[category].Add(fullPath);
        CustomHandDict[category].Sort(WindowsLogicalComparer.StrCmpLogicalW);

        CustomHandChange?.Invoke(null, new(fullPath, category));
    }

    public static void InitializeSceneDirectories()
    {
        SceneDirectoryList.Clear();
        SceneDirectoryList.Add(SceneDirectory);

        foreach (var directory in Directory.GetDirectories(ScenesPath))
            SceneDirectoryList.Add(new DirectoryInfo(directory).Name);

        SceneDirectoryList.Sort((a, b) => KeepAtTop(a, b, SceneDirectory));
    }

    public static void InitializeKankyoDirectories()
    {
        KankyoDirectoryList.Clear();
        KankyoDirectoryList.Add(KankyoDirectory);

        foreach (var directory in Directory.GetDirectories(KankyoPath))
            KankyoDirectoryList.Add(new DirectoryInfo(directory).Name);

        KankyoDirectoryList.Sort((a, b) => KeepAtTop(a, b, KankyoDirectory));
    }

    public static void InitializePoses()
    {
        // Load Poses
        var poseListPath = Path.Combine(DatabasePath, "mm_pose_list.json");

        try
        {
            var poseListJson = File.ReadAllText(poseListPath);

            foreach (var poseList in JsonConvert.DeserializeObject<List<SerializePoseList>>(poseListJson))
            {
                PoseDict[poseList.UIName] = poseList.PoseList;
                PoseGroupList.Add(poseList.UIName);
            }
        }
        catch (IOException e)
        {
            Utility.LogError($"Could not open pose database because {e.Message}");
            Utility.LogMessage("Pose list will be serverely limited.");
            AddDefaultPose();
        }
        catch (Exception e)
        {
            Utility.LogError($"Could not parse pose database because {e.Message}");
            AddDefaultPose();
        }

        // Get Other poses that'll go into Normal, Normal 2 and Ero 2
        var com3d2MotionList = GameUty.FileSystem.GetList("motion", AFileSystemBase.ListType.AllFile);

        if (com3d2MotionList?.Length > 0)
        {
            var poseSet = new HashSet<string>();

            foreach (var poses in PoseDict.Values)
                poseSet.UnionWith(poses);

            var newCategories = new[] { "normal2", "ero2" };

            foreach (var category in newCategories)
                if (!PoseDict.ContainsKey(category))
                    PoseDict[category] = new();

            // TODO: Try to group these poses into more than "normal2" and "ero2"
            foreach (var path in com3d2MotionList)
            {
                if (Path.GetExtension(path) is not ".anm")
                    continue;

                var file = Path.GetFileNameWithoutExtension(path);

                if (poseSet.Contains(file))
                    continue;

                if (file.StartsWith("edit_"))
                {
                    PoseDict["normal"].Add(file);
                }
                else if (file is not ("dance_cm3d2_001_zoukin" or "dance_cm3d2_001_mop" or "aruki_1_idougo_f"
                    or "sleep2" or "stand_akire2") && !file.EndsWith("_3_") && !file.EndsWith("_5_")
                    && !file.StartsWith("vr_") && !file.StartsWith("dance_mc") && !file.Contains("_kubi_")
                    && !file.Contains("a01_") && !file.Contains("b01_") && !file.Contains("b02_")
                    && !file.EndsWith("_m2") && !file.EndsWith("_m2_once_") && !file.StartsWith("h_")
                    && !file.StartsWith("event_") && !file.StartsWith("man_") && !file.EndsWith("_m")
                    && !file.Contains("_m_") && !file.Contains("_man_"))
                {
                    if (path.Contains(@"\sex\"))
                        PoseDict["ero2"].Add(file);
                    else
                        PoseDict["normal2"].Add(file);
                }
            }

            foreach (var category in newCategories)
            {
                if (PoseDict[category].Count > 0)
                {
                    if (!PoseGroupList.Contains(category))
                        PoseGroupList.Add(category);
                }
                else
                {
                    PoseDict.Remove(category);
                }
            }
        }

        InitializeCustomPoses();

        static void AddDefaultPose()
        {
            if (!PoseDict.ContainsKey("normal"))
                PoseDict["normal"] = new()
                {
                    "maid_stand01",
                };

            if (!PoseGroupList.Contains("normal"))
                PoseGroupList.Insert(0, "normal");
        }
    }

    public static void InitializeCustomPoses()
    {
        CustomPoseGroupList.Clear();
        CustomPoseDict.Clear();

        CustomPoseGroupList.Add(CustomPoseDirectory);
        CustomPoseDict[CustomPoseDirectory] = new();

        GetPoses(CustomPosePath);

        foreach (var directory in Directory.GetDirectories(CustomPosePath))
            GetPoses(directory);

        CustomPoseGroupList.Sort((a, b) => KeepAtTop(a, b, CustomPoseDirectory));

        CustomPoseChange?.Invoke(null, PresetChangeEventArgs.Empty);

        static void GetPoses(string directory)
        {
            var poseList = Directory.GetFiles(directory)
                .Where(file => Path.GetExtension(file) is ".anm");

            if (!poseList.Any())
                return;

            var poseGroupName = new DirectoryInfo(directory).Name;

            if (poseGroupName is not CustomPoseDirectory)
                CustomPoseGroupList.Add(poseGroupName);

            CustomPoseDict[poseGroupName] = poseList.ToList();
            CustomPoseDict[poseGroupName].Sort(WindowsLogicalComparer.StrCmpLogicalW);
        }
    }

    public static void InitializeHandPresets()
    {
        CustomHandGroupList.Clear();
        CustomHandDict.Clear();

        CustomHandGroupList.Add(CustomHandDirectory);
        CustomHandDict[CustomHandDirectory] = new();

        GetPresets(CustomHandPath);

        foreach (var directory in Directory.GetDirectories(CustomHandPath))
            GetPresets(directory);

        CustomHandGroupList.Sort((a, b) => KeepAtTop(a, b, CustomHandDirectory));

        CustomHandChange?.Invoke(null, PresetChangeEventArgs.Empty);

        static void GetPresets(string directory)
        {
            var presetList = Directory.GetFiles(directory)
                .Where(file => Path.GetExtension(file) is ".xml");

            if (!presetList.Any())
                return;

            var presetCategory = new DirectoryInfo(directory).Name;

            if (presetCategory is not CustomHandDirectory)
                CustomHandGroupList.Add(presetCategory);

            CustomHandDict[presetCategory] = presetList.ToList();
            CustomHandDict[presetCategory].Sort(WindowsLogicalComparer.StrCmpLogicalW);
        }
    }

    public static void InitializeFaceBlends()
    {
        PhotoFaceData.Create();

        FaceGroupList.AddRange(PhotoFaceData.popup_category_list.Select(kvp => kvp.Key));

        foreach (var kvp in PhotoFaceData.category_list)
            FaceDict[kvp.Key] = kvp.Value.ConvertAll(data => data.setting_name);

        InitializeCustomFaceBlends();
    }

    public static void InitializeCustomFaceBlends()
    {
        CustomFaceGroupList.Clear();
        CustomFaceDict.Clear();

        CustomFaceGroupList.Add(CustomFaceDirectory);
        CustomFaceDict[CustomFaceDirectory] = new();

        GetFacePresets(CustomFacePath);

        foreach (var directory in Directory.GetDirectories(CustomFacePath))
            GetFacePresets(directory);

        CustomFaceGroupList.Sort((a, b) => KeepAtTop(a, b, CustomFaceDirectory));

        CustomFaceChange?.Invoke(null, PresetChangeEventArgs.Empty);

        static void GetFacePresets(string directory)
        {
            IEnumerable<string> presetList = Directory.GetFiles(directory)
                .Where(file => Path.GetExtension(file) is ".xml").ToList();

            if (!presetList.Any())
                return;

            var faceGroupName = new DirectoryInfo(directory).Name;

            if (faceGroupName is not CustomFaceDirectory)
                CustomFaceGroupList.Add(faceGroupName);

            CustomFaceDict[faceGroupName] = presetList.ToList();
            CustomFaceDict[faceGroupName].Sort(WindowsLogicalComparer.StrCmpLogicalW);
        }
    }

    public static void InitializeBGs()
    {
        // Load BGs
        PhotoBGData.Create();

        // COM3D2 BGs
        foreach (var bgData in PhotoBGData.data)
        {
            if (string.IsNullOrEmpty(bgData.create_prefab_name))
                continue;

            var bg = bgData.create_prefab_name;

            BGList.Add(bg);
        }

        // CM3D2 BGs
        if (GameUty.IsEnabledCompatibilityMode)
        {
            using var csvParser = OpenCsvParser("phot_bg_list.nei", GameUty.FileSystemOld);

            for (var cell_y = 1; cell_y < csvParser.max_cell_y; cell_y++)
            {
                if (!csvParser.IsCellToExistData(3, cell_y))
                    continue;

                var bg = csvParser.GetCellAsString(3, cell_y);

                BGList.Add(bg);
            }
        }

        // Set index regardless of there being myRoom bgs or not
        MyRoomCustomBGIndex = BGList.Count;

        var saveDataDict = CreativeRoomManager.GetSaveDataDic();

        if (saveDataDict is not null)
            MyRoomCustomBGList.AddRange(saveDataDict);
    }

    public static void InitializeDogu()
    {
        foreach (var customCategory in CustomDoguCategories.Values)
            DoguDict[customCategory] = new();

        InitializeDeskItems();
        InitializePhotoBGItems();
        InitializeOtherDogu();
        InitializeHandItems();

        foreach (var category in PhotoBGObjectData.popup_category_list.Select(kvp => kvp.Key))
        {
            if (category is "マイオブジェクト")
                continue;

            DoguCategories.Add(category);
        }

        foreach (DoguCategory category in Enum.GetValues(typeof(DoguCategory)))
            DoguCategories.Add(CustomDoguCategories[category]);
    }

    public static List<ModItem> GetModPropList(string category)
    {
        if (!PropManager.ModItemsOnly && !MenuFilesReady)
        {
            Utility.LogMessage("Menu files are not ready yet");

            return null;
        }

        if (!MenuFilesInitialized)
            InitializeModProps();

        if (!ModPropDict.ContainsKey(category))
            return null;

        var selectedList = ModPropDict[category];

        if (!selectedList[0].Icon)
        {
            selectedList.Sort((a, b) =>
            {
                var res = a.Priority.CompareTo(b.Priority);

                if (res is 0)
                    res = string.Compare(a.Name, b.Name);

                return res;
            });

            var previousMenuFile = string.Empty;

            selectedList.RemoveAll(item =>
            {
                if (item.Icon)
                    return false;

                Texture2D icon;
                var iconFile = item.IconFile;

                if (string.IsNullOrEmpty(iconFile))
                {
                    // TODO: Remove '{iconFile}' since it will not add anymore information.
                    Utility.LogWarning($"Could not find icon '{iconFile}' for menu '{item.MenuFile}");

                    return true;
                }

                try
                {
                    icon = ImportCM.CreateTexture(iconFile);
                }
                catch
                {
                    try
                    {
                        icon = ImportCM.CreateTexture($"tex\\{iconFile}");
                    }
                    catch
                    {
                        Utility.LogWarning($"Could not load '{iconFile}' for menu '{item.MenuFile}");

                        return true;
                    }
                }

                item.Icon = icon;

                return false;
            });
        }

        return selectedList;
    }

    private static void InitializeOtherDogu()
    {
        DoguDict[CustomDoguCategories[DoguCategory.BGSmall]] = BGList;
        DoguDict[CustomDoguCategories[DoguCategory.Mob]].AddRange(
            new[]
            {
                "Mob_Man_Stand001", "Mob_Man_Stand002", "Mob_Man_Stand003", "Mob_Man_Sit001", "Mob_Man_Sit002",
                "Mob_Man_Sit003", "Mob_Girl_Stand001", "Mob_Girl_Stand002", "Mob_Girl_Stand003", "Mob_Girl_Sit001",
                "Mob_Girl_Sit002", "Mob_Girl_Sit003",
            });

        var otherDoguList = DoguDict[CustomDoguCategories[DoguCategory.Other]];

        // bg object extend
        var doguHashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        doguHashSet.UnionWith(BGList);

        try
        {
            var ignoreListPath = Path.Combine(DatabasePath, "bg_ignore_list.json");
            var ignoreListJson = File.ReadAllText(ignoreListPath);
            var ignoreList = JsonConvert.DeserializeObject<string[]>(ignoreListJson);

            doguHashSet.UnionWith(ignoreList);
        }
        catch (IOException e)
        {
            Utility.LogWarning($"Could not open ignored BG database because {e.Message}");
        }
        catch
        {
            // Ignored.
        }

        foreach (var doguList in DoguDict.Values)
            doguHashSet.UnionWith(doguList);

        foreach (var path in GameUty.FileSystem.GetList("bg", AFileSystemBase.ListType.AllFile))
        {
            if (Path.GetExtension(path) is not ".asset_bg" || path.Contains("myroomcustomize"))
                continue;

            var file = Path.GetFileNameWithoutExtension(path);

            if (doguHashSet.Contains(file) || file.EndsWith("_hit"))
                continue;

            otherDoguList.Add(file);
            doguHashSet.Add(file);
        }

        // Get cherry picked dogu that I can't find in the game files
        try
        {
            var doguExtendPath = Path.Combine(DatabasePath, "extra_dogu.json");
            var doguExtendJson = File.ReadAllText(doguExtendPath);

            otherDoguList.AddRange(JsonConvert.DeserializeObject<IEnumerable<string>>(doguExtendJson));
        }
        catch (IOException e)
        {
            Utility.LogError($"Could not open extra prop database because {e.Message}");
        }
        catch (Exception e)
        {
            Utility.LogError($"Could not parse extra prop database because {e.Message}");
        }

        foreach (var path in GameUty.FileSystemOld.GetList("bg", AFileSystemBase.ListType.AllFile))
        {
            if (Path.GetExtension(path) is not ".asset_bg")
                continue;

            var file = Path.GetFileNameWithoutExtension(path);

            if (!doguHashSet.Contains(file) && !file.EndsWith("_not_optimisation"))
                otherDoguList.Add(file);
        }
    }

    private static void InitializeDeskItems()
    {
        var enabledIDs = new HashSet<int>();

        CsvCommonIdManager.ReadEnabledIdList(
            CsvCommonIdManager.FileSystemType.Normal, true, "desk_item_enabled_id", ref enabledIDs);

        CsvCommonIdManager.ReadEnabledIdList(
            CsvCommonIdManager.FileSystemType.Old, true, "desk_item_enabled_id", ref enabledIDs);

        var com3d2DeskDogu = DoguDict[CustomDoguCategories[DoguCategory.Desk]];

        GetDeskItems(GameUty.FileSystem);

        void GetDeskItems(AFileSystemBase fs)
        {
            using var csvParser = OpenCsvParser("desk_item_detail.nei", fs);

            for (var cell_y = 1; cell_y < csvParser.max_cell_y; cell_y++)
            {
                if (!csvParser.IsCellToExistData(0, cell_y))
                    continue;

                var cell = csvParser.GetCellAsInteger(0, cell_y);

                if (!enabledIDs.Contains(cell))
                    continue;

                var dogu = string.Empty;

                if (csvParser.IsCellToExistData(3, cell_y))
                    dogu = csvParser.GetCellAsString(3, cell_y);
                else if (csvParser.IsCellToExistData(4, cell_y))
                    dogu = csvParser.GetCellAsString(4, cell_y);

                if (!string.IsNullOrEmpty(dogu))
                    com3d2DeskDogu.Add(dogu);
            }
        }
    }

    private static void InitializePhotoBGItems()
    {
        PhotoBGObjectData.Create();

        var photoBGObjectList = PhotoBGObjectData.data;

        var doguCategories = new List<string>();
        var addedCategories = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var photoBGObject in photoBGObjectList)
        {
            var category = photoBGObject.category;

            if (!addedCategories.Contains(category))
            {
                addedCategories.Add(category);
                doguCategories.Add(category);
            }

            if (!DoguDict.ContainsKey(category))
                DoguDict[category] = new();

            var dogu = string.Empty;

            if (!string.IsNullOrEmpty(photoBGObject.create_prefab_name))
                dogu = photoBGObject.create_prefab_name;
            else if (!string.IsNullOrEmpty(photoBGObject.create_asset_bundle_name))
                dogu = photoBGObject.create_asset_bundle_name;
            else if (!string.IsNullOrEmpty(photoBGObject.direct_file))
                dogu = photoBGObject.direct_file;

            if (!string.IsNullOrEmpty(dogu))
                DoguDict[category].Add(dogu);
        }

        DoguDict["パーティクル"].AddRange(
            new[]
            {
                "Particle/pLineY", "Particle/pLineP02", "Particle/pHeart01",
                "Particle/pLine_act2", "Particle/pstarY_act3",
            });
    }

    private static void InitializeHandItems()
    {
        if (HandItemsInitialized)
            return;

        if (!MenuFilesReady)
        {
            if (!beginHandItemInit)
                MenuFilesReadyChange += (_, _) =>
                    InitializeHandItems();

            beginHandItemInit = true;

            return;
        }

        var menuDataBase = GameMain.Instance.MenuDataBase;
        var doguHashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        doguHashSet.UnionWith(BGList);

        try
        {
            var ignoreListPath = Path.Combine(DatabasePath, "bg_ignore_list.json");
            var ignoreListJson = File.ReadAllText(ignoreListPath);
            var ignoreList = JsonConvert.DeserializeObject<string[]>(ignoreListJson);

            doguHashSet.UnionWith(ignoreList);
        }
        catch (IOException e)
        {
            Utility.LogWarning($"Could not open ignored BG database because {e.Message}");
        }
        catch (Exception e)
        {
            Utility.LogError($"Could not parse ignored BG database because {e.Message}");
        }

        foreach (var doguList in DoguDict.Values)
            doguHashSet.UnionWith(doguList);

        var category = CustomDoguCategories[DoguCategory.HandItem];

        for (var i = 0; i < menuDataBase.GetDataSize(); i++)
        {
            menuDataBase.SetIndex(i);

            if ((MPN)menuDataBase.GetCategoryMpn() is not MPN.handitem)
                continue;

            var menuFileName = menuDataBase.GetMenuFileName();

            if (menuDataBase.GetBoDelOnly() || menuFileName.EndsWith("_del.menu"))
                continue;

            var handItemAsOdogu = Utility.HandItemToOdogu(menuFileName);
            var isolatedHandItem = menuFileName.Substring(menuFileName.IndexOf('_') + 1);

            if (doguHashSet.Contains(handItemAsOdogu) || doguHashSet.Contains(isolatedHandItem))
                continue;

            doguHashSet.Add(isolatedHandItem);
            DoguDict[category].Add(menuFileName);

            // Check for a half deck of cards to add the full deck as well
            if (menuFileName is "handitemd_cards_i_.menu")
                DoguDict[category].Add("handiteml_cards_i_.menu");
        }

        HandItemsInitialized = true;
        OnMenuFilesChange(MenuFilesEventArgs.EventType.HandItems);
    }

    private static void InitializeMpnAttachProps()
    {
        if (MpnAttachInitialized)
            return;

        if (!MenuFilesReady)
        {
            if (beginMpnAttachInit)
                return;

            beginMpnAttachInit = true;

            MenuFilesReadyChange += (_, _) =>
                InitializeMpnAttachProps();

            return;
        }

        var menuDataBase = GameMain.Instance.MenuDataBase;
        var attachMpn = new[] { MPN.kousoku_lower, MPN.kousoku_upper };

        for (var i = 0; i < menuDataBase.GetDataSize(); i++)
        {
            menuDataBase.SetIndex(i);

            var itemMpn = (MPN)menuDataBase.GetCategoryMpn();

            if (!attachMpn.Any(mpn => mpn == itemMpn))
                continue;

            var menuFileName = menuDataBase.GetMenuFileName();
            var mpnTag = menuDataBase.GetCategoryMpnText();

            if (menuDataBase.GetBoDelOnly() || menuFileName.EndsWith("_del.menu"))
                continue;

            MpnAttachPropList.Add(new(itemMpn, menuFileName));
        }

        MpnAttachInitialized = true;
        OnMenuFilesChange(MenuFilesEventArgs.EventType.MpnAttach);
    }

    private static void InitializeMyRoomProps()
    {
        PlacementData.CreateData();

        var myRoomData = PlacementData.GetAllDatas(false);

        myRoomData.Sort(MyRoomDataComparator);

        foreach (var data in myRoomData)
        {
            var category = PlacementData.GetCategoryName(data.categoryID);

            if (!MyRoomPropDict.ContainsKey(category))
            {
                MyRoomPropCategories.Add(category);
                MyRoomPropDict[category] = new();
            }

            var asset = !string.IsNullOrEmpty(data.resourceName) ? data.resourceName : data.assetName;
            var item = new MyRoomItem()
            {
                PrefabName = asset,
                ID = data.ID,
            };

            MyRoomPropDict[category].Add(item);
        }

        static int MyRoomDataComparator(PlacementData.Data a, PlacementData.Data b)
        {
            var res = a.categoryID.CompareTo(b.categoryID);

            if (res is 0)
                res = a.ID.CompareTo(b.ID);

            return res;
        }
    }

    private static void InitializeModProps()
    {
        for (var i = 1; i < MenuCategories.Length; i++)
            ModPropDict[MenuCategories[i]] = new();

        if (!PropManager.ModItemsOnly)
        {
            var menuDatabase = GameMain.Instance.MenuDataBase;

            for (var i = 0; i < menuDatabase.GetDataSize(); i++)
            {
                menuDatabase.SetIndex(i);

                var modItem = new ModItem();

                if (ParseNativeMenuFile(i, modItem))
                    ModPropDict[modItem.Category].Add(modItem);
            }
        }

        var cache = new MenuFileCache();

        foreach (var modMenuFile in GameUty.ModOnlysMenuFiles)
        {
            ModItem modItem;

            if (cache.Has(modMenuFile))
            {
                modItem = cache[modMenuFile];
            }
            else
            {
                modItem = ModItem.Mod(modMenuFile);
                ParseMenuFile(modMenuFile, modItem);
                cache[modMenuFile] = modItem;
            }

            if (ValidBG2MenuFile(modItem))
                ModPropDict[modItem.Category].Add(modItem);
        }

        cache.Serialize();

        foreach (var modFile in Menu.GetModFiles())
        {
            var modItem = ModItem.OfficialMod(modFile);

            if (ParseModMenuFile(modFile, modItem))
                ModPropDict[modItem.Category].Add(modItem);
        }

        MenuFilesInitialized = true;
    }

    // TODO: This could leak on failure.
    private static CsvParser OpenCsvParser(string nei, AFileSystemBase fs)
    {
        try
        {
            if (!fs.IsExistentFile(nei))
                return null;

            var file = fs.FileOpen(nei);
            var csvParser = new CsvParser();

            if (csvParser.Open(file))
                return csvParser;

            file?.Dispose();

            return null;
        }
        catch
        {
            // Ignored.
        }

        return null;
    }

    private static void OnMenuFilesChange(MenuFilesEventArgs.EventType eventType) =>
        MenuFilesChange?.Invoke(null, new(eventType));

    private static int KeepAtTop(string a, string b, string topItem)
    {
        if (a == b)
            return 0;

        if (a == topItem)
            return -1;

        if (b == topItem)
            return 1;

        return WindowsLogicalComparer.StrCmpLogicalW(a, b);
    }

    private class SerializePoseList
    {
        public string UIName { get; set; }

        public List<string> PoseList { get; set; }
    }
}
