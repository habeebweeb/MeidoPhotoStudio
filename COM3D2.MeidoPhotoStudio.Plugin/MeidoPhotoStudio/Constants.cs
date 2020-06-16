using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using wf;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Constants
    {
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
        public static readonly List<string> PoseGroupList;
        public static readonly Dictionary<string, List<string>> PoseDict;
        public static readonly Dictionary<string, List<KeyValuePair<string, string>>> CustomPoseDict;
        public static int CustomPoseGroupsIndex { get; private set; } = -1;
        public static int MyRoomCustomBGIndex { get; private set; } = -1;
        public static readonly List<string> FaceBlendList;
        public static readonly List<string> BGList;
        public static readonly List<KeyValuePair<string, string>> MyRoomCustomBGList;
        public static readonly List<string> DoguList;
        public static readonly List<string> OtherDoguList;

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

            PoseDict = new Dictionary<string, List<string>>();
            PoseGroupList = new List<string>();
            CustomPoseDict = new Dictionary<string, List<KeyValuePair<string, string>>>();

            FaceBlendList = new List<string>();

            BGList = new List<string>();
            MyRoomCustomBGList = new List<KeyValuePair<string, string>>();
            DoguList = new List<string>();
            OtherDoguList = new List<string>();
        }

        public static void Initialize()
        {
            MakeDirectories();
            InitializePoses();
            InitializeFaceBlends();
            InitializeBGs();
            InitializeDogu();
        }

        public static void MakeDirectories()
        {
            foreach (string directory in new[] { customPosePath, scenesPath, kankyoPath, configPath })
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            }
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
            InitializeDeskItems();
            InitializePhotoBGItems();
            // InitializeHandItems();
        }

        private static void InitializeDeskItems()
        {
            // enabled id
            HashSet<int> enabledIDs = new HashSet<int>();
            CsvCommonIdManager.ReadEnabledIdList(
                CsvCommonIdManager.FileSystemType.Normal, true, "desk_item_enabled_id", ref enabledIDs
            );
            CsvCommonIdManager.ReadEnabledIdList(
                CsvCommonIdManager.FileSystemType.Old, true, "desk_item_enabled_id", ref enabledIDs
            );

            List<string> com3d2DeskDogu = new List<string>(new[] {
                "Mob_Man_Stand001", "Mob_Man_Stand002", "Mob_Man_Stand003", "Mob_Man_Sit001", "Mob_Man_Sit002",
                "Mob_Man_Sit003", "Mob_Girl_Stand001", "Mob_Girl_Stand002", "Mob_Girl_Stand003", "Mob_Girl_Sit001",
                "Mob_Girl_Sit002", "Mob_Girl_Sit003", "Salon:65", "Salon:63", "Salon:69"
            });

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
            // GetDeskItems(GameUty.FileSystemOld);

            OtherDoguList.AddRange(com3d2DeskDogu);
        }

        private static void InitializePhotoBGItems()
        {
            PhotoBGObjectData.Create();
            List<PhotoBGObjectData> photoBGObjectList = PhotoBGObjectData.data;

            List<string> particleList = new List<string>();

            List<string> doguPrefabList = new List<string>();
            List<string> doguAssetList = new List<string>();
            List<string> directFileList = new List<string>();

            foreach (PhotoBGObjectData photoBgObject in photoBGObjectList)
            {
                if (!string.IsNullOrEmpty(photoBgObject.create_prefab_name))
                {
                    List<string> list = photoBgObject.category == "パーティクル"
                        ? particleList
                        : doguPrefabList;
                    list.Add(photoBgObject.create_prefab_name);
                }
                else if (!string.IsNullOrEmpty(photoBgObject.create_asset_bundle_name))
                {
                    doguAssetList.Add(photoBgObject.create_asset_bundle_name);
                }
                else if (!string.IsNullOrEmpty(photoBgObject.direct_file))
                {
                    directFileList.Add(photoBgObject.direct_file);
                }
            }

            OtherDoguList.AddRange(new[] {
                "Particle/pLineY", "Particle/pLineP02", "Particle/pHeart01",
                "Particle/pLine_act2", "Particle/pstarY_act2"
            });

            OtherDoguList.AddRange(particleList);

            DoguList.AddRange(doguPrefabList);
            DoguList.AddRange(doguAssetList);
            DoguList.AddRange(directFileList);

            string ignoreListPath = Path.Combine(configPath, "mm_ignore_list.json");
            string ignoreListJson = File.ReadAllText(ignoreListPath);
            string[] ignoreList = JsonConvert.DeserializeObject<IEnumerable<string>>(ignoreListJson).ToArray();

            // bg object extend
            HashSet<string> doguHashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string bg in BGList)
            {
                doguHashSet.Add(bg);
            }
            foreach (string bg in ignoreList)
            {
                doguHashSet.Add(bg);
            }
            foreach (string dogu in DoguList)
            {
                doguHashSet.Add(dogu);
            }
            foreach (string dogu in OtherDoguList)
            {
                doguHashSet.Add(dogu);
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

        private static void InitializeHandItems()
        {
            List<string> handItems = new List<string>(
                GameUty.MenuFiles.Where(menu => menu.StartsWith("handiteml") || menu.StartsWith("handitemr"))
            );

            WriteToFile("mm_hand_items", handItems);
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

        public class SerializePoseList
        {
            public string UIName { get; set; }
            public List<string> PoseList { get; set; }
        }
    }
}
