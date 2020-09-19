using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using MyRoomCustom;
using UnityEngine;
using wf;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;
    internal static class Constants
    {
        private static bool beginHandItemInit;
        private static bool beginMpnAttachInit;
        public const string customPoseDirectory = "Custom Poses";
        public const string customHandDirectory = "Hand Presets";
        public const string customFaceDirectory = "Face Presets";
        public const string sceneDirectory = "Scenes";
        public const string kankyoDirectory = "Environments";
        public const string configDirectory = "MeidoPhotoStudio";
        public const string translationDirectory = "Translations";
        public static readonly string customPosePath;
        public static readonly string customHandPath;
        public static readonly string customFacePath;
        public static readonly string scenesPath;
        public static readonly string kankyoPath;
        public static readonly string configPath;
        public static readonly int mainWindowID = 765;
        public static readonly int messageWindowID = 961;
        public static readonly int sceneManagerWindowID = 876;
        public static readonly int sceneManagerModalID = 283;
        public static readonly int dropdownWindowID = 777;
        public enum Window
        {
            Call, Pose, Face, BG, BG2, Main, Message, Save, SaveModal, Settings
        }
        public enum Scene
        {
            Daily = 3, Edit = 5
        }
        public static readonly List<string> PoseGroupList = new List<string>();
        public static readonly Dictionary<string, List<string>> PoseDict = new Dictionary<string, List<string>>();
        public static readonly List<string> CustomPoseGroupList = new List<string>();
        public static readonly Dictionary<string, List<string>> CustomPoseDict = new Dictionary<string, List<string>>();
        public static readonly List<string> CustomHandGroupList = new List<string>();
        public static readonly Dictionary<string, List<string>> CustomHandDict = new Dictionary<string, List<string>>();
        public static readonly List<string> FaceGroupList = new List<string>();
        public static readonly Dictionary<string, List<string>> FaceDict = new Dictionary<string, List<string>>();
        public static readonly List<string> CustomFaceGroupList = new List<string>();
        public static readonly Dictionary<string, List<string>> CustomFaceDict = new Dictionary<string, List<string>>();
        public static readonly List<string> BGList = new List<string>();
        public static readonly List<KeyValuePair<string, string>> MyRoomCustomBGList
            = new List<KeyValuePair<string, string>>();
        public static readonly List<string> DoguCategories = new List<string>();
        public static readonly Dictionary<string, List<string>> DoguDict = new Dictionary<string, List<string>>();
        public static readonly List<string> MyRoomPropCategories = new List<string>();
        public static readonly Dictionary<string, List<MyRoomItem>> MyRoomPropDict
            = new Dictionary<string, List<MyRoomItem>>();
        public static readonly Dictionary<string, List<ModItem>> ModPropDict
            = new Dictionary<string, List<ModItem>>(StringComparer.InvariantCultureIgnoreCase);
        public static readonly List<string> SceneDirectoryList = new List<string>();
        public static readonly List<string> KankyoDirectoryList = new List<string>();
        public static readonly List<MpnAttachProp> MpnAttachPropList = new List<MpnAttachProp>();
        public static int MyRoomCustomBGIndex { get; private set; } = -1;
        public static bool HandItemsInitialized { get; private set; }
        public static bool MpnAttachInitialized { get; private set; }
        public static bool MenuFilesInitialized { get; private set; }
        public static event EventHandler<MenuFilesEventArgs> MenuFilesChange;
        public static event EventHandler<CustomPoseEventArgs> CustomPoseChange;
        public static event EventHandler<CustomPoseEventArgs> CustomHandChange;
        public static event EventHandler<CustomPoseEventArgs> CustomFaceChange;
        public enum DoguCategory
        {
            Other, Mob, Desk, HandItem, BGSmall
        }
        public static readonly Dictionary<DoguCategory, string> customDoguCategories =
            new Dictionary<DoguCategory, string>()
            {
                [DoguCategory.Other] = "other",
                [DoguCategory.Mob] = "mob",
                [DoguCategory.Desk] = "desk",
                [DoguCategory.HandItem] = "handItem",
                [DoguCategory.BGSmall] = "bgSmall"
            };

        static Constants()
        {
            configPath = Path.Combine(BepInEx.Paths.ConfigPath, configDirectory);

            string presetPath = Path.Combine(configPath, "Presets");

            customPosePath = Path.Combine(presetPath, customPoseDirectory);
            customHandPath = Path.Combine(presetPath, customHandDirectory);
            customFacePath = Path.Combine(presetPath, customFaceDirectory);
            scenesPath = Path.Combine(configPath, sceneDirectory);
            kankyoPath = Path.Combine(configPath, kankyoDirectory);

            string[] directories = new[] {
                customPosePath, customHandPath, scenesPath, kankyoPath, configPath, customFacePath
            };

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            }
        }

        public static void Initialize()
        {
            InitializeScenes();
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

            if (string.IsNullOrEmpty(filename)) filename = "face_preset";
            if (directory.Equals(customFaceDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                directory = string.Empty;
            }
            directory = Path.Combine(customFacePath, directory);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string fullPath = Path.Combine(directory, filename);

            if (File.Exists($"{fullPath}.xml")) fullPath += $"_{DateTime.Now:yyyyMMddHHmmss}";

            fullPath = Path.GetFullPath($"{fullPath}.xml");

            if (!fullPath.StartsWith(customFacePath))
            {
                Utility.LogError($"Could not save face preset! Path is invalid: '{fullPath}'");
                return;
            }

            XElement rootElement = new XElement("FaceData");

            foreach (KeyValuePair<string, float> kvp in faceData)
            {
                rootElement.Add(new XElement("elm", kvp.Value.ToString("G9"), new XAttribute("name", kvp.Key)));
            }

            XDocument fullDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                new XComment("MeidoPhotoStudio Face Preset"),
                rootElement
            );

            fullDocument.Save(fullPath);

            FileInfo fileInfo = new FileInfo(fullPath);

            string category = fileInfo.Directory.Name;
            string faceGroup = CustomFaceGroupList.Find(
                group => string.Equals(category, group, StringComparison.InvariantCultureIgnoreCase)
            );

            if (string.IsNullOrEmpty(faceGroup))
            {
                CustomFaceGroupList.Add(category);
                CustomFaceDict[category] = new List<string>();
            }
            else category = faceGroup;

            CustomFaceDict[category].Add(fullPath);
            CustomFaceDict[category].Sort();

            CustomFaceChange?.Invoke(null, new CustomPoseEventArgs(fullPath, category));
        }

        public static void AddPose(byte[] anmBinary, string filename, string directory)
        {
            // TODO: Consider writing a file system monitor

            filename = Utility.SanitizePathPortion(filename);
            directory = Utility.SanitizePathPortion(directory);
            if (string.IsNullOrEmpty(filename)) filename = "custom_pose";
            if (directory.Equals(customPoseDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                directory = string.Empty;
            }
            directory = Path.Combine(customPosePath, directory);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string fullPath = Path.Combine(directory, filename);

            if (File.Exists($"{fullPath}.anm")) fullPath += $"_{DateTime.Now:yyyyMMddHHmmss}";

            fullPath = Path.GetFullPath($"{fullPath}.anm");

            if (!fullPath.StartsWith(customPosePath))
            {
                Utility.LogError($"Could not save pose! Path is invalid: '{fullPath}'");
                return;
            }

            File.WriteAllBytes(fullPath, anmBinary);

            FileInfo fileInfo = new FileInfo(fullPath);

            string category = fileInfo.Directory.Name;
            string poseGroup = CustomPoseGroupList.Find(
                group => string.Equals(category, group, StringComparison.InvariantCultureIgnoreCase)
            );

            if (string.IsNullOrEmpty(poseGroup))
            {
                CustomPoseGroupList.Add(category);
                CustomPoseDict[category] = new List<string>();
            }
            else category = poseGroup;

            CustomPoseDict[category].Add(fullPath);
            CustomPoseDict[category].Sort();

            CustomPoseChange?.Invoke(null, new CustomPoseEventArgs(fullPath, category));
        }

        public static void AddHand(byte[] handBinary, bool right, string filename, string directory)
        {
            filename = Utility.SanitizePathPortion(filename);
            directory = Utility.SanitizePathPortion(directory);
            if (string.IsNullOrEmpty(filename)) filename = "custom_hand";
            if (directory.Equals(customHandDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                directory = string.Empty;
            }
            directory = Path.Combine(customHandPath, directory);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string fullPath = Path.Combine(directory, filename);

            if (File.Exists($"{fullPath}.xml")) fullPath += $"_{DateTime.Now:yyyyMMddHHmmss}";

            fullPath = Path.GetFullPath($"{fullPath}.xml");

            if (!fullPath.StartsWith(customHandPath))
            {
                Utility.LogError($"Could not save hand! Path is invalid: '{fullPath}'");
                return;
            }

            XDocument finalXml = new XDocument(new XDeclaration("1.0", "utf-8", "true"),
                new XComment("CM3D2 FingerData"),
                new XElement("FingerData",
                    new XElement("GameVersion", Misc.GAME_VERSION),
                    new XElement("RightData", right),
                    new XElement("BinaryData", Convert.ToBase64String(handBinary))
                )
            );

            finalXml.Save(fullPath);

            FileInfo fileInfo = new FileInfo(fullPath);

            string category = fileInfo.Directory.Name;
            string handGroup = CustomHandGroupList.Find(
                group => string.Equals(category, group, StringComparison.InvariantCultureIgnoreCase)
            );

            if (string.IsNullOrEmpty(handGroup))
            {
                CustomHandGroupList.Add(category);
                CustomHandDict[category] = new List<string>();
            }
            else category = handGroup;

            CustomHandDict[category].Add(fullPath);
            CustomHandDict[category].Sort();

            CustomHandChange?.Invoke(null, new CustomPoseEventArgs(fullPath, category));
        }

        public static void InitializeScenes()
        {
            SceneDirectoryList.Clear();
            KankyoDirectoryList.Clear();

            SceneDirectoryList.Add(sceneDirectory);
            foreach (string directory in Directory.GetDirectories(scenesPath))
            {
                SceneDirectoryList.Add(new DirectoryInfo(directory).Name);
            }

            KankyoDirectoryList.Add(kankyoDirectory);
            foreach (string directory in Directory.GetDirectories(kankyoPath))
            {
                KankyoDirectoryList.Add(new DirectoryInfo(directory).Name);
            }
        }

        public static void InitializePoses()
        {
            // Load Poses
            string poseListJson = File.ReadAllText(Path.Combine(configPath, "Database\\mm_pose_list.json"));
            foreach (SerializePoseList poseList in JsonConvert.DeserializeObject<List<SerializePoseList>>(poseListJson))
            {
                PoseDict[poseList.UIName] = poseList.PoseList;
                PoseGroupList.Add(poseList.UIName);
            }

            // Get Other poses that'll go into Normal 2 and Ero 2
            string[] com3d2MotionList = GameUty.FileSystem.GetList("motion", AFileSystemBase.ListType.AllFile);

            if (com3d2MotionList?.Length > 0)
            {
                HashSet<string> poseSet = new HashSet<string>();
                foreach (List<string> poses in PoseDict.Values)
                {
                    poseSet.UnionWith(poses);
                }

                List<string> editPoseList = new List<string>();
                List<string> otherPoseList = new List<string>();
                List<string> eroPoseList = new List<string>();
                foreach (string path in com3d2MotionList)
                {
                    if (Path.GetExtension(path) == ".anm")
                    {
                        string file = Path.GetFileNameWithoutExtension(path);
                        if (!poseSet.Contains(file))
                        {
                            if (file.StartsWith("edit_"))
                            {
                                editPoseList.Add(file);
                            }
                            else if (file != "dance_cm3d2_001_zoukin" && file != "dance_cm3d2_001_mop"
                                && file != "aruki_1_idougo_f" && file != "sleep2" && file != "stand_akire2"
                                && !file.EndsWith("_3_") && !file.EndsWith("_5_") && !file.StartsWith("vr_")
                                && !file.StartsWith("dance_mc") && !file.Contains("_kubi_") && !file.Contains("a01_")
                                && !file.Contains("b01_") && !file.Contains("b02_") && !file.EndsWith("_m2")
                                && !file.EndsWith("_m2_once_") && !file.StartsWith("h_") && !file.StartsWith("event_")
                                && !file.StartsWith("man_") && !file.EndsWith("_m") && !file.Contains("_m_")
                                && !file.Contains("_man_")
                            )
                            {
                                if (path.Contains(@"\sex\")) eroPoseList.Add(file);
                                else otherPoseList.Add(file);
                            }
                        }
                    }
                }
                PoseDict["normal"].AddRange(editPoseList);
                PoseDict["normal2"] = otherPoseList;
                PoseDict["ero2"] = eroPoseList;

                PoseGroupList.AddRange(new[] { "normal2", "ero2" });
            }

            void GetPoses(string directory)
            {
                List<string> poseList = Directory.GetFiles(directory)
                    .Where(file => Path.GetExtension(file) == ".anm").ToList();

                if (poseList.Count > 0)
                {
                    string poseGroupName = new DirectoryInfo(directory).Name;
                    if (poseGroupName != customPoseDirectory) CustomPoseGroupList.Add(poseGroupName);
                    CustomPoseDict[poseGroupName] = poseList;
                }
            }

            CustomPoseGroupList.Add(customPoseDirectory);
            CustomPoseDict[customPoseDirectory] = new List<string>();

            GetPoses(customPosePath);

            foreach (string directory in Directory.GetDirectories(customPosePath))
            {
                GetPoses(directory);
            }
        }

        public static void InitializeHandPresets()
        {
            void GetPresets(string directory)
            {
                IEnumerable<string> presetList = Directory.GetFiles(directory)
                    .Where(file => Path.GetExtension(file) == ".xml");

                if (presetList.Any())
                {
                    string presetCategory = new DirectoryInfo(directory).Name;
                    if (presetCategory != customHandDirectory) CustomHandGroupList.Add(presetCategory);
                    CustomHandDict[presetCategory] = new List<string>(presetList);
                }
            }

            CustomHandGroupList.Add(customHandDirectory);
            CustomHandDict[customHandDirectory] = new List<string>();

            GetPresets(customHandPath);

            foreach (string directory in Directory.GetDirectories(customHandPath))
            {
                GetPresets(directory);
            }
        }

        public static void InitializeFaceBlends()
        {
            PhotoFaceData.Create();

            FaceGroupList.AddRange(PhotoFaceData.popup_category_list.Select(kvp => kvp.Key));

            foreach (KeyValuePair<string, List<PhotoFaceData>> kvp in PhotoFaceData.category_list)
            {
                FaceDict[kvp.Key] = kvp.Value.Select(data => data.setting_name).ToList();
            }

            void GetFacePresets(string directory)
            {
                List<string> presetList = Directory.GetFiles(directory)
                    .Where(file => Path.GetExtension(file) == ".xml").ToList();

                if (presetList.Count > 0)
                {
                    string faceGroupName = new DirectoryInfo(directory).Name;
                    if (faceGroupName != customFaceDirectory) CustomFaceGroupList.Add(faceGroupName);
                    CustomFaceDict[faceGroupName] = presetList;
                }
            }

            CustomFaceGroupList.Add(customFaceDirectory);
            CustomFaceDict[customFaceDirectory] = new List<string>();

            GetFacePresets(customFacePath);

            foreach (string directory in Directory.GetDirectories(customFacePath))
            {
                GetFacePresets(directory);
            }
        }

        public static void InitializeBGs()
        {
            // Load BGs
            PhotoBGData.Create();

            // COM3D2 BGs
            foreach (PhotoBGData bgData in PhotoBGData.data)
            {
                if (!string.IsNullOrEmpty(bgData.create_prefab_name))
                {
                    string bg = bgData.create_prefab_name;
                    BGList.Add(bg);
                }
            }

            // CM3D2 BGs
            if (GameUty.IsEnabledCompatibilityMode)
            {
                using (CsvParser csvParser = OpenCsvParser("phot_bg_list.nei", GameUty.FileSystemOld))
                {
                    for (int cell_y = 1; cell_y < csvParser.max_cell_y; cell_y++)
                    {
                        if (csvParser.IsCellToExistData(3, cell_y))
                        {
                            string bg = csvParser.GetCellAsString(3, cell_y);
                            BGList.Add(bg);
                        }
                    }
                }
            }

            Dictionary<string, string> saveDataDict = CreativeRoomManager.GetSaveDataDic();

            if (saveDataDict != null)
            {
                MyRoomCustomBGIndex = BGList.Count;
                MyRoomCustomBGList.AddRange(saveDataDict);
            }
        }

        public static void InitializeDogu()
        {
            foreach (string customCategory in customDoguCategories.Values)
            {
                DoguDict[customCategory] = new List<string>();
            }

            InitializeDeskItems();
            InitializePhotoBGItems();
            InitializeOtherDogu();
            InitializeHandItems();

            foreach (string category in PhotoBGObjectData.popup_category_list.Select(kvp => kvp.Key))
            {
                if (category == "マイオブジェクト") continue;
                DoguCategories.Add(category);
            }

            foreach (DoguCategory category in Enum.GetValues(typeof(DoguCategory)))
            {
                DoguCategories.Add(customDoguCategories[category]);
            }
        }

        private static void InitializeOtherDogu()
        {
            DoguDict[customDoguCategories[DoguCategory.BGSmall]] = BGList;

            DoguDict[customDoguCategories[DoguCategory.Mob]].AddRange(new[] {
                "Mob_Man_Stand001", "Mob_Man_Stand002", "Mob_Man_Stand003", "Mob_Man_Sit001", "Mob_Man_Sit002",
                "Mob_Man_Sit003", "Mob_Girl_Stand001", "Mob_Girl_Stand002", "Mob_Girl_Stand003", "Mob_Girl_Sit001",
                "Mob_Girl_Sit002", "Mob_Girl_Sit003", "Salon:65", "Salon:63", "Salon:69"
            });

            List<string> DoguList = DoguDict[customDoguCategories[DoguCategory.Other]];

            string ignoreListPath = Path.Combine(configPath, "Database\\bg_ignore_list.json");
            string ignoreListJson = File.ReadAllText(ignoreListPath);
            string[] ignoreList = JsonConvert.DeserializeObject<string[]>(ignoreListJson);

            // bg object extend
            HashSet<string> doguHashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            doguHashSet.UnionWith(BGList);
            doguHashSet.UnionWith(ignoreList);
            foreach (List<string> doguList in DoguDict.Values)
            {
                doguHashSet.UnionWith(doguList);
            }

            foreach (string path in GameUty.FileSystem.GetList("bg", AFileSystemBase.ListType.AllFile))
            {
                if (Path.GetExtension(path) == ".asset_bg" && !path.Contains("myroomcustomize"))
                {
                    string file = Path.GetFileNameWithoutExtension(path);
                    if (!doguHashSet.Contains(file) && !file.EndsWith("_hit"))
                    {
                        DoguList.Add(file);
                        doguHashSet.Add(file);
                    }
                }
            }

            // Get cherry picked dogu that I can't find in the game files
            string doguExtendPath = Path.Combine(configPath, "Database\\extra_dogu.json");
            string doguExtendJson = File.ReadAllText(doguExtendPath);

            DoguList.AddRange(JsonConvert.DeserializeObject<IEnumerable<string>>(doguExtendJson));

            foreach (string path in GameUty.FileSystemOld.GetList("bg", AFileSystemBase.ListType.AllFile))
            {
                if (Path.GetExtension(path) == ".asset_bg")
                {
                    string file = Path.GetFileNameWithoutExtension(path);
                    if (!doguHashSet.Contains(file) && !file.EndsWith("_not_optimisation"))
                    {
                        DoguList.Add(file);
                    }
                }
            }
        }

        private static void InitializeDeskItems()
        {
            HashSet<int> enabledIDs = new HashSet<int>();
            CsvCommonIdManager.ReadEnabledIdList(
                CsvCommonIdManager.FileSystemType.Normal, true, "desk_item_enabled_id", ref enabledIDs
            );
            CsvCommonIdManager.ReadEnabledIdList(
                CsvCommonIdManager.FileSystemType.Old, true, "desk_item_enabled_id", ref enabledIDs
            );

            List<string> com3d2DeskDogu = DoguDict[customDoguCategories[DoguCategory.Desk]];

            void GetDeskItems(AFileSystemBase fs)
            {
                using (CsvParser csvParser = OpenCsvParser("desk_item_detail.nei", fs))
                {
                    for (int cell_y = 1; cell_y < csvParser.max_cell_y; cell_y++)
                    {
                        if (csvParser.IsCellToExistData(0, cell_y))
                        {
                            int cell = csvParser.GetCellAsInteger(0, cell_y);
                            if (enabledIDs.Contains(cell))
                            {
                                string dogu = string.Empty;
                                if (csvParser.IsCellToExistData(3, cell_y))
                                {
                                    dogu = csvParser.GetCellAsString(3, cell_y);
                                }
                                else if (csvParser.IsCellToExistData(4, cell_y))
                                {
                                    dogu = csvParser.GetCellAsString(4, cell_y);
                                }

                                if (!string.IsNullOrEmpty(dogu))
                                {
                                    com3d2DeskDogu.Add(dogu);
                                }
                            }
                        }
                    }
                }
            }

            GetDeskItems(GameUty.FileSystem);
        }

        private static void InitializePhotoBGItems()
        {
            PhotoBGObjectData.Create();
            List<PhotoBGObjectData> photoBGObjectList = PhotoBGObjectData.data;

            List<string> doguCategories = new List<string>();
            HashSet<string> addedCategories = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (PhotoBGObjectData photoBGObject in photoBGObjectList)
            {
                string category = photoBGObject.category;

                if (!addedCategories.Contains(category))
                {
                    addedCategories.Add(category);
                    doguCategories.Add(category);
                }

                if (!DoguDict.ContainsKey(category))
                {
                    DoguDict[category] = new List<string>();
                }

                string dogu = string.Empty;
                if (!string.IsNullOrEmpty(photoBGObject.create_prefab_name))
                {
                    dogu = photoBGObject.create_prefab_name;
                }
                else if (!string.IsNullOrEmpty(photoBGObject.create_asset_bundle_name))
                {
                    dogu = photoBGObject.create_asset_bundle_name;
                }
                else if (!string.IsNullOrEmpty(photoBGObject.direct_file))
                {
                    dogu = photoBGObject.direct_file;
                }

                if (!string.IsNullOrEmpty(dogu))
                {
                    DoguDict[category].Add(dogu);
                }
            }

            DoguDict["パーティクル"].AddRange(new[] {
                "Particle/pLineY", "Particle/pLineP02", "Particle/pHeart01",
                "Particle/pLine_act2", "Particle/pstarY_act2"
            });
        }

        private static void InitializeHandItems()
        {
            if (HandItemsInitialized) return;

            if (!MenuFilesReady)
            {
                if (!beginHandItemInit) MenuFilesReadyChange += (s, a) => InitializeHandItems();
                beginHandItemInit = true;
                return;
            }

            MenuDataBase menuDataBase = GameMain.Instance.MenuDataBase;

            string ignoreListJson = File.ReadAllText(Path.Combine(configPath, "Database\\bg_ignore_list.json"));
            string[] ignoreList = JsonConvert.DeserializeObject<IEnumerable<string>>(ignoreListJson).ToArray();

            HashSet<string> doguHashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            doguHashSet.UnionWith(BGList);
            doguHashSet.UnionWith(ignoreList);
            foreach (List<string> doguList in DoguDict.Values)
            {
                doguHashSet.UnionWith(doguList);
            }

            string category = customDoguCategories[DoguCategory.HandItem];
            for (int i = 0; i < menuDataBase.GetDataSize(); i++)
            {
                menuDataBase.SetIndex(i);
                if ((MPN)menuDataBase.GetCategoryMpn() == MPN.handitem)
                {
                    string menuFileName = menuDataBase.GetMenuFileName();
                    if (menuDataBase.GetBoDelOnly() || menuFileName.EndsWith("_del.menu")) continue;

                    string handItemAsOdogu = Utility.HandItemToOdogu(menuFileName);
                    string isolatedHandItem = menuFileName.Substring(menuFileName.IndexOf('_') + 1);

                    if (!doguHashSet.Contains(handItemAsOdogu) && !doguHashSet.Contains(isolatedHandItem))
                    {
                        doguHashSet.Add(isolatedHandItem);
                        DoguDict[category].Add(menuFileName);

                        // Check for a half deck of cards to add the full deck as well
                        if (menuFileName == "handitemd_cards_i_.menu")
                        {
                            DoguDict[category].Add("handiteml_cards_i_.menu");
                        }
                    }
                }
            }

            HandItemsInitialized = true;
            OnMenuFilesChange(MenuFilesEventArgs.EventType.HandItems);
        }

        private static void InitializeMpnAttachProps()
        {
            if (MpnAttachInitialized) return;

            if (!MenuFilesReady)
            {
                if (!beginMpnAttachInit) MenuFilesReadyChange += (s, a) => InitializeMpnAttachProps();
                beginMpnAttachInit = true;
                return;
            }

            MenuDataBase menuDataBase = GameMain.Instance.MenuDataBase;

            MPN[] attachMpn = { MPN.kousoku_lower, MPN.kousoku_upper };

            for (int i = 0; i < menuDataBase.GetDataSize(); i++)
            {
                menuDataBase.SetIndex(i);
                MPN itemMpn = (MPN)menuDataBase.GetCategoryMpn();

                if (attachMpn.Any(mpn => mpn == itemMpn))
                {
                    string menuFileName = menuDataBase.GetMenuFileName();
                    string mpnTag = menuDataBase.GetCategoryMpnText();

                    if (menuDataBase.GetBoDelOnly() || menuFileName.EndsWith("_del.menu")) continue;

                    MpnAttachPropList.Add(new MpnAttachProp(itemMpn, menuFileName));
                }
            }

            MpnAttachInitialized = true;
            OnMenuFilesChange(MenuFilesEventArgs.EventType.MpnAttach);
        }

        private static void InitializeMyRoomProps()
        {
            PlacementData.CreateData();
            List<PlacementData.Data> myRoomData = PlacementData.GetAllDatas(false);
            myRoomData.Sort((a, b) =>
            {
                int res = a.categoryID.CompareTo(b.categoryID);
                if (res == 0) res = a.ID.CompareTo(b.ID);
                return res;
            });

            foreach (PlacementData.Data data in myRoomData)
            {
                string category = PlacementData.GetCategoryName(data.categoryID);

                if (!MyRoomPropDict.ContainsKey(category))
                {
                    MyRoomPropCategories.Add(category);
                    MyRoomPropDict[category] = new List<MyRoomItem>();
                }

                string asset = !string.IsNullOrEmpty(data.resourceName) ? data.resourceName : data.assetName;

                MyRoomItem item = new MyRoomItem() { PrefabName = asset, ID = data.ID };

                MyRoomPropDict[category].Add(item);
            }
        }

        private static void InitializeModProps()
        {
            for (int i = 1; i < MenuCategories.Length; i++)
            {
                ModPropDict[MenuCategories[i]] = new List<ModItem>();
            }

            if (!PropManager.ModItemsOnly)
            {
                MenuDataBase menuDatabase = GameMain.Instance.MenuDataBase;

                for (int i = 0; i < menuDatabase.GetDataSize(); i++)
                {
                    menuDatabase.SetIndex(i);
                    ModItem modItem = new ModItem();
                    if (ParseNativeMenuFile(i, modItem))
                    {
                        ModPropDict[modItem.Category].Add(modItem);
                    }
                }
            }

            MenuFileCache cache = new MenuFileCache();

            foreach (string modMenuFile in GameUty.ModOnlysMenuFiles)
            {
                ModItem modItem;
                if (cache.Has(modMenuFile)) modItem = cache[modMenuFile];
                else
                {
                    modItem = ModItem.Mod(modMenuFile);
                    ParseMenuFile(modMenuFile, modItem);
                    cache[modMenuFile] = modItem;
                }
                if (ValidBG2MenuFile(modItem)) ModPropDict[modItem.Category].Add(modItem);
            }

            cache.Serialize();

            foreach (string modFile in Menu.GetModFiles())
            {
                ModItem modItem = ModItem.OfficialMod(modFile);
                if (ParseModMenuFile(modFile, modItem))
                {
                    ModPropDict[modItem.Category].Add(modItem);
                }
            }
            MenuFilesInitialized = true;
        }

        public static List<ModItem> GetModPropList(string category)
        {
            if (!PropManager.ModItemsOnly && !MenuFilesReady)
            {
                Utility.LogMessage("Menu files are not ready yet");
                return null;
            }

            if (!MenuFilesInitialized) InitializeModProps();

            if (!ModPropDict.ContainsKey(category)) return null;

            List<ModItem> selectedList = ModPropDict[category];

            if (selectedList[0].Icon == null)
            {
                selectedList.Sort((a, b) =>
                {
                    int res = a.Priority.CompareTo(b.Priority);
                    if (res == 0) res = string.Compare(a.Name, b.Name);
                    return res;
                });

                string previousMenuFile = string.Empty;
                selectedList.RemoveAll(item =>
                {
                    if (item.Icon == null)
                    {
                        Texture2D icon;
                        string iconFile = item.IconFile;
                        if (string.IsNullOrEmpty(iconFile) || !GameUty.FileSystem.IsExistentFile(iconFile))
                        {
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
                    }
                    return false;
                });
            }

            return selectedList;
        }

        private static CsvParser OpenCsvParser(string nei, AFileSystemBase fs)
        {
            try
            {
                if (fs.IsExistentFile(nei))
                {
                    AFileBase file = fs.FileOpen(nei);
                    CsvParser csvParser = new CsvParser();
                    if (csvParser.Open(file)) return csvParser;

                    file?.Dispose();
                }
            }
            catch { }
            return null;
        }

        private static void OnMenuFilesChange(MenuFilesEventArgs.EventType eventType)
        {
            MenuFilesChange?.Invoke(null, new MenuFilesEventArgs(eventType));
        }

        private class SerializePoseList
        {
            public string UIName { get; set; }
            public List<string> PoseList { get; set; }
        }
    }

    public class MenuFilesEventArgs : EventArgs
    {
        public EventType Type { get; }
        public enum EventType
        {
            HandItems, MenuFiles, MpnAttach
        }
        public MenuFilesEventArgs(EventType type) => Type = type;
    }

    public class CustomPoseEventArgs : EventArgs
    {
        public string Category { get; }
        public string Path { get; }
        public CustomPoseEventArgs(string path, string category)
        {
            Path = path;
            Category = category;
        }
    }

    public struct MpnAttachProp
    {
        public MPN Tag { get; }
        public string MenuFile { get; }

        public MpnAttachProp(MPN tag, string menuFile)
        {
            Tag = tag;
            MenuFile = menuFile;
        }
    }
}
