using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using MyRoomCustom;
using UnityEngine;
using wf;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;
    internal class Constants
    {
        private static bool beginHandItemInit;
        public static readonly string customPosePath;
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
            Call, Pose, Face, BG, BG2, Main, Message, Save, SaveModal
        }
        public enum Scene
        {
            Daily = 3, Edit = 5
        }
        public static readonly List<string> PoseGroupList = new List<string>();
        public static readonly Dictionary<string, List<string>> PoseDict = new Dictionary<string, List<string>>();
        public static readonly Dictionary<string, List<KeyValuePair<string, string>>> CustomPoseDict
            = new Dictionary<string, List<KeyValuePair<string, string>>>();
        public static readonly List<string> FaceBlendList = new List<string>();
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
        public static int CustomPoseGroupsIndex { get; private set; } = -1;
        public static int MyRoomCustomBGIndex { get; private set; } = -1;
        public static bool HandItemsInitialized { get; private set; } = false;
        public static bool MenuFilesInitialized { get; private set; } = false;
        public static event EventHandler<MenuFilesEventArgs> MenuFilesChange;
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
            string modsPath = Path.Combine(Path.GetFullPath(".\\"), @"Mod\MeidoPhotoStudio");
            customPosePath = Path.Combine(modsPath, "Custom Poses");
            scenesPath = Path.Combine(modsPath, "Scenes");
            kankyoPath = Path.Combine(modsPath, "Environments");
            configPath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                @"Config\MeidoPhotoStudio"
            );

            foreach (string directory in new[] { customPosePath, scenesPath, kankyoPath, configPath })
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            }
        }

        public static void Initialize()
        {
            InitializePoses();
            InitializeFaceBlends();
            InitializeBGs();
            InitializeDogu();
            InitializeMyRoomProps();
        }

        public static void InitializePoses()
        {
            // Load Poses
            string poseListJson = File.ReadAllText(Path.Combine(configPath, "mm_pose_list.json"));
            List<SerializePoseList> poseLists = JsonConvert.DeserializeObject<List<SerializePoseList>>(poseListJson);

            foreach (SerializePoseList poseList in poseLists)
            {
                PoseDict[poseList.UIName] = poseList.PoseList;
                PoseGroupList.Add(poseList.UIName);
            }

            // Get Other poses that'll go into Normal 2 and Ero 2
            string[] com3d2MotionList = GameUty.FileSystem.GetList("motion", AFileSystemBase.ListType.AllFile);

            if (com3d2MotionList != null && com3d2MotionList.Length > 0)
            {
                HashSet<string> poseSet = new HashSet<string>();
                foreach (KeyValuePair<string, List<string>> poses in PoseDict)
                {
                    foreach (string pose in poses.Value)
                    {
                        poseSet.Add(pose);
                    }
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
                                if (!path.Contains(@"\sex\")) otherPoseList.Add(file);
                                else eroPoseList.Add(file);
                            }
                        }
                    }
                }
                PoseDict["normal"].AddRange(editPoseList);
                PoseDict["normal2"] = otherPoseList;
                PoseDict["ero2"] = eroPoseList;

                PoseGroupList.AddRange(new[] { "normal2", "ero2" });
            }

            CustomPoseGroupsIndex = PoseGroupList.Count;

            Action<string> GetPoses = directory =>
            {
                List<KeyValuePair<string, string>> poseList = new List<KeyValuePair<string, string>>();
                foreach (string file in Directory.GetFiles(directory))
                {
                    if (Path.GetExtension(file) == ".anm")
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        poseList.Add(new KeyValuePair<string, string>(fileName, file));
                    }
                }
                if (poseList.Count > 0)
                {
                    string poseGroupName = new DirectoryInfo(directory).Name;
                    PoseGroupList.Add(poseGroupName);
                    CustomPoseDict[poseGroupName] = poseList;
                }
            };

            GetPoses(customPosePath);

            foreach (string directory in Directory.GetDirectories(customPosePath))
            {
                GetPoses(directory);
            }
        }

        public static void InitializeFaceBlends()
        {
            using (CsvParser csvParser = OpenCsvParser("phot_face_list.nei"))
            {
                for (int cell_y = 1; cell_y < csvParser.max_cell_y; cell_y++)
                {
                    if (csvParser.IsCellToExistData(3, cell_y))
                    {
                        string blendValue = csvParser.GetCellAsString(3, cell_y);
                        FaceBlendList.Add(blendValue);
                    }
                }
            }
        }

        public static void InitializeBGs()
        {
            // Load BGs
            PhotoBGData.Create();
            List<PhotoBGData> photList = PhotoBGData.data;

            // COM3D2 BGs
            foreach (PhotoBGData bgData in photList)
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

            Dictionary<string, string> saveDataDict = MyRoomCustom.CreativeRoomManager.GetSaveDataDic();

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

            foreach (KeyValuePair<string, UnityEngine.Object> keyValuePair in PhotoBGObjectData.popup_category_list)
            {
                string category = keyValuePair.Key;
                if (category == "マイオブジェクト") continue;
                DoguCategories.Add(keyValuePair.Key);
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

            string ignoreListPath = Path.Combine(configPath, "mm_ignore_list.json");
            string ignoreListJson = File.ReadAllText(ignoreListPath);
            string[] ignoreList = JsonConvert.DeserializeObject<IEnumerable<string>>(ignoreListJson).ToArray();

            // bg object extend
            HashSet<string> doguHashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            doguHashSet.UnionWith(BGList);
            doguHashSet.UnionWith(ignoreList);
            foreach (KeyValuePair<string, List<string>> keyValuePair in DoguDict)
            {
                doguHashSet.UnionWith(keyValuePair.Value);
            }

            string[] com3d2BgList = GameUty.FileSystem.GetList("bg", AFileSystemBase.ListType.AllFile);
            foreach (string path in com3d2BgList)
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
            string doguExtendPath = Path.Combine(configPath, "mm_dogu_extend.json");
            string doguExtendJson = File.ReadAllText(doguExtendPath);

            DoguList.AddRange(JsonConvert.DeserializeObject<IEnumerable<string>>(doguExtendJson));

            string[] cm3d2BgList = GameUty.FileSystemOld.GetList("bg", AFileSystemBase.ListType.AllFile);
            foreach (string path in cm3d2BgList)
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

            Action<AFileSystemBase> GetDeskItems = fs =>
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
                                string dogu = String.Empty;
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
            };

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

                string dogu = String.Empty;
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

            if (!MenuFileUtility.MenuFilesReady)
            {
                if (!beginHandItemInit) MenuFileUtility.MenuFilesReadyChange += (s, a) => InitializeHandItems();
                beginHandItemInit = true;
                return;
            }

            MenuDataBase menuDataBase = GameMain.Instance.MenuDataBase;

            string ignoreListJson = File.ReadAllText(Path.Combine(configPath, "mm_ignore_list.json"));
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

            OnMenuFilesChange(MenuFilesEventArgs.HandItems);
            HandItemsInitialized = true;
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
            // TODO: cache menu files
            for (int i = 1; i < MenuFileUtility.MenuCategories.Length; i++)
            {
                ModPropDict[MenuCategories[i]] = new List<ModItem>();
            }

            if (!Configuration.ModItemsOnly)
            {
                MenuDataBase menuDatabase = GameMain.Instance.MenuDataBase;

                for (int i = 0; i < menuDatabase.GetDataSize(); i++)
                {
                    menuDatabase.SetIndex(i);
                    ModItem modItem = new ModItem();
                    if (MenuFileUtility.ParseNativeMenuFile(i, modItem))
                    {
                        ModPropDict[modItem.Category].Add(modItem);
                    }
                }
            }

            MenuFileCache cache = new MenuFileCache();

            foreach (string modMenuFile in GameUty.ModOnlysMenuFiles)
            {
                ModItem modItem;
                if (cache.Has(modMenuFile))
                {
                    modItem = cache[modMenuFile];
                }
                else
                {
                    modItem = new ModItem() { MenuFile = modMenuFile, IsMod = true };
                    MenuFileUtility.ParseMenuFile(modMenuFile, modItem);
                    cache[modMenuFile] = modItem;
                }
                if (MenuFileUtility.ValidBG2MenuFile(modItem)) ModPropDict[modItem.Category].Add(modItem);
            }

            cache.Serialize();

            foreach (string modFile in Menu.GetModFiles())
            {
                ModItem modItem = new ModItem()
                {
                    MenuFile = modFile,
                    IsMod = true,
                    IsOfficialMod = true,
                    Priority = 1000f
                };
                if (ParseModMenuFile(modFile, modItem))
                {
                    ModPropDict[modItem.Category].Add(modItem);
                }
            }
            MenuFilesInitialized = true;
        }

        public static List<ModItem> GetModPropList(string category)
        {
            if (!Configuration.ModItemsOnly)
            {
                if (!MenuFileUtility.MenuFilesReady)
                {
                    Debug.Log("Menu files are not ready yet");
                    return null;
                }
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

                string previousMenuFile = String.Empty;
                selectedList.RemoveAll(item =>
                {
                    if (item.Icon == null)
                    {
                        Texture2D icon;
                        string iconFile = item.IconFile;
                        if (string.IsNullOrEmpty(iconFile) || !GameUty.FileSystem.IsExistentFile(iconFile))
                        {
                            Debug.LogWarning($"Could not find icon '{iconFile}' for menu '{item.MenuFile}");
                            return true;
                        }
                        else
                        {
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
                                    Debug.LogWarning($"Could not load '{iconFile}' for menu '{item.MenuFile}");
                                    return true;
                                }
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
                    else file?.Dispose();
                }
            }
            catch { }
            return null;
        }

        private static CsvParser OpenCsvParser(string nei)
        {
            return OpenCsvParser(nei, GameUty.FileSystem);
        }

        public static void WriteToFile(string name, IEnumerable<string> list)
        {
            if (Path.GetExtension(name) != ".txt") name += ".txt";
            File.WriteAllLines(Path.Combine(configPath, name), list.ToArray());
        }

        private static void OnMenuFilesChange(MenuFilesEventArgs args)
        {
            MenuFilesChange?.Invoke(null, args);
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
            HandItems, MenuFiles
        }
        public static MenuFilesEventArgs HandItems => new MenuFilesEventArgs(EventType.HandItems);
        public static MenuFilesEventArgs MenuFiles => new MenuFilesEventArgs(EventType.MenuFiles);

        public MenuFilesEventArgs(EventType type)
        {
            this.Type = type;
        }
    }
}
