using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx.Configuration;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class SceneManager : IManager
    {
        public static bool Busy { get; private set; }
        public static readonly Vector2 sceneDimensions = new Vector2(480, 270);
        private static readonly ConfigEntry<bool> sortDescending;
        private static readonly ConfigEntry<SortMode> currentSortMode;
        private readonly MeidoPhotoStudio meidoPhotoStudio;
        private int SortDirection => SortDescending ? -1 : 1;
        public bool Initialized { get; private set; }
        public bool KankyoMode { get; set; }
        public bool SortDescending
        {
            get => sortDescending.Value;
            set => sortDescending.Value = value;
        }
        public List<Scene> SceneList { get; } = new List<Scene>();
        public int CurrentDirectoryIndex { get; private set; } = -1;
        public string CurrentDirectoryName => CurrentDirectoryList[CurrentDirectoryIndex];
        public List<string> CurrentDirectoryList
            => KankyoMode ? Constants.KankyoDirectoryList : Constants.SceneDirectoryList;
        public string CurrentBasePath => KankyoMode ? Constants.kankyoPath : Constants.scenesPath;
        public string CurrentScenesDirectory
            => CurrentDirectoryIndex == 0 ? CurrentBasePath : Path.Combine(CurrentBasePath, CurrentDirectoryName);
        public SortMode CurrentSortMode
        {
            get => currentSortMode.Value;
            private set => currentSortMode.Value = value;
        }
        public int CurrentSceneIndex { get; private set; } = -1;
        public Scene CurrentScene => SceneList.Count == 0 ? null : SceneList[CurrentSceneIndex];
        public enum SortMode
        {
            Name, DateCreated, DateModified
        }

        static SceneManager()
        {
            sortDescending = Configuration.Config.Bind(
                "SceneManager", "SortDescending",
                false,
                "Sort scenes descending (Z-A)"
            );

            currentSortMode = Configuration.Config.Bind(
                "SceneManager", "SortMode",
                SortMode.Name,
                "Scene sorting mode"
            );

            Input.Register(MpsKey.OpenSceneManager, KeyCode.F8, "Hide/show scene manager");
            Input.Register(MpsKey.SaveScene, KeyCode.S, "Quick save scene");
            Input.Register(MpsKey.LoadScene, KeyCode.A, "Load quick saved scene");
        }

        public SceneManager(MeidoPhotoStudio meidoPhotoStudio)
        {
            this.meidoPhotoStudio = meidoPhotoStudio;
            Activate();
        }

        public void Activate() { }

        public void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;
                SelectDirectory(0);
            }
        }

        public void Deactivate() => ClearSceneList();

        public void Update()
        {
            if (Input.Control)
            {
                if (Input.GetKeyDown(MpsKey.SaveScene)) QuickSaveScene();
                else if (Input.GetKeyDown(MpsKey.LoadScene)) QuickLoadScene();
            }
        }

        public void DeleteDirectory()
        {
            if (Directory.Exists(CurrentScenesDirectory)) Directory.Delete(CurrentScenesDirectory, true);

            CurrentDirectoryList.RemoveAt(CurrentDirectoryIndex);
            CurrentDirectoryIndex = Mathf.Clamp(CurrentDirectoryIndex, 0, CurrentDirectoryList.Count - 1);
            UpdateSceneList();
        }

        public void OverwriteScene() => SaveScene(overwrite: true);

        public void ToggleKankyoMode()
        {
            KankyoMode = !KankyoMode;
            CurrentDirectoryIndex = 0;
            UpdateSceneList();
        }

        public void SaveScene(bool overwrite = false)
        {
            if (Busy) return;
            Busy = true;

            if (!Directory.Exists(CurrentScenesDirectory)) Directory.CreateDirectory(CurrentScenesDirectory);

            MeidoPhotoStudio.NotifyRawScreenshot += SaveScene;

            MeidoPhotoStudio.TakeScreenshot(new ScreenshotEventArgs() { InMemory = true });

            void SaveScene(object sender, ScreenshotEventArgs args)
            {
                MeidoPhotoStudio.NotifyRawScreenshot -= SaveScene;
                meidoPhotoStudio.StartCoroutine(SaveSceneToFile(args.Screenshot, overwrite));
            }
        }

        public void SelectDirectory(int directoryIndex)
        {
            directoryIndex = Mathf.Clamp(directoryIndex, 0, CurrentDirectoryList.Count - 1);

            if (directoryIndex == CurrentDirectoryIndex) return;

            CurrentDirectoryIndex = directoryIndex;

            UpdateSceneList();
        }

        public void SelectScene(int sceneIndex)
        {
            CurrentSceneIndex = Mathf.Clamp(sceneIndex, 0, SceneList.Count - 1);
            CurrentScene.GetNumberOfMaids();
        }

        public void AddDirectory(string directoryName)
        {
            directoryName = Utility.SanitizePathPortion(directoryName);

            if (!CurrentDirectoryList.Contains(directoryName, StringComparer.InvariantCultureIgnoreCase))
            {
                string finalPath = Path.Combine(CurrentBasePath, directoryName);
                string fullPath = Path.GetFullPath(finalPath);

                if (!fullPath.StartsWith(CurrentBasePath))
                {
                    string baseDirectoryName = KankyoMode ? Constants.kankyoDirectory : Constants.sceneDirectory;
                    Utility.LogError($"Could not add directory to {baseDirectoryName}. Path is invalid: '{fullPath}'");
                    return;
                }

                CurrentDirectoryList.Add(directoryName);
                Directory.CreateDirectory(finalPath);

                UpdateDirectoryList();
                CurrentDirectoryIndex = CurrentDirectoryList.IndexOf(directoryName);

                UpdateSceneList();
            }
        }

        public void Refresh()
        {
            if (!Directory.Exists(CurrentScenesDirectory)) CurrentDirectoryIndex = 0;

            if (KankyoMode) Constants.InitializeKankyoDirectories();
            else Constants.InitializeSceneDirectories();

            UpdateSceneList();
        }

        public void SortScenes(SortMode sortMode)
        {
            CurrentSortMode = sortMode;
            Comparison<Scene> comparator;
            switch (CurrentSortMode)
            {
                case SortMode.DateModified: comparator = SortByDateModified; break;
                case SortMode.DateCreated: comparator = SortByDateCreated; break;
                default: comparator = SortByName; break;
            }
            SceneList.Sort(comparator);
        }

        public void DeleteScene()
        {
            if (CurrentScene.FileInfo.Exists)
            {
                CurrentScene.FileInfo.Delete();
            }
            SceneList.RemoveAt(CurrentSceneIndex);
            CurrentSceneIndex = Mathf.Clamp(CurrentSceneIndex, 0, SceneList.Count - 1);
        }

        public void LoadScene()
        {
            meidoPhotoStudio.ApplyScene(CurrentScene.FileInfo.FullName);
        }

        private int SortByName(Scene a, Scene b)
        {
            return SortDirection * LexicographicStringComparer.Comparison(a.FileInfo.Name, b.FileInfo.Name);
        }

        private int SortByDateCreated(Scene a, Scene b)
        {
            return SortDirection * DateTime.Compare(a.FileInfo.CreationTime, b.FileInfo.CreationTime);
        }

        private int SortByDateModified(Scene a, Scene b)
        {
            return SortDirection * DateTime.Compare(a.FileInfo.LastWriteTime, b.FileInfo.LastWriteTime);
        }

        private void UpdateSceneList()
        {
            ClearSceneList();

            if (!Directory.Exists(CurrentScenesDirectory))
            {
                Directory.CreateDirectory(CurrentScenesDirectory);
            }

            foreach (string filename in Directory.GetFiles(CurrentScenesDirectory))
            {
                if (Path.GetExtension(filename) == ".png") SceneList.Add(new Scene(filename));
            }

            SortScenes(CurrentSortMode);

            CurrentSceneIndex = Mathf.Clamp(CurrentSceneIndex, 0, SceneList.Count - 1);
        }

        private void UpdateDirectoryList()
        {
            string baseDirectoryName = KankyoMode ? Constants.kankyoDirectory : Constants.sceneDirectory;
            CurrentDirectoryList.Sort((a, b)
                => a.Equals(baseDirectoryName, StringComparison.InvariantCultureIgnoreCase) ? -1 : a.CompareTo(b));
        }

        private void ClearSceneList()
        {
            foreach (Scene scene in SceneList) scene.Destroy();
            SceneList.Clear();
        }

        private void QuickSaveScene()
        {
            if (Busy) return;
            byte[] data = meidoPhotoStudio.SerializeScene(kankyo: false);
            if (data == null) return;
            File.WriteAllBytes(Path.Combine(Constants.configPath, "mpstempscene"), data);
        }

        private void QuickLoadScene()
        {
            if (Busy) return;
            meidoPhotoStudio.ApplyScene(Path.Combine(Constants.configPath, "mpstempscene"));
        }

        private System.Collections.IEnumerator SaveSceneToFile(Texture2D screenshot, bool overwrite = false)
        {
            Busy = true;

            byte[] sceneData = meidoPhotoStudio.SerializeScene(KankyoMode);

            if (sceneData != null)
            {
                string scenePrefix = KankyoMode ? "mpskankyo" : "mpsscene";
                string fileName = $"{scenePrefix}{Utility.Timestamp}.png";
                string savePath = Path.Combine(CurrentScenesDirectory, fileName);

                if (overwrite && CurrentScene != null) savePath = CurrentScene.FileInfo.FullName;
                else overwrite = false;

                Utility.ResizeToFit(screenshot, (int)sceneDimensions.x, (int)sceneDimensions.y);

                using (FileStream fileStream = File.Create(savePath))
                {
                    byte[] encodedPng = screenshot.EncodeToPNG();
                    fileStream.Write(encodedPng, 0, encodedPng.Length);
                    fileStream.Write(sceneData, 0, sceneData.Length);
                }

                UnityEngine.Object.DestroyImmediate(screenshot);

                if (overwrite)
                {
                    File.SetCreationTime(savePath, CurrentScene.FileInfo.CreationTime);
                    CurrentScene.Destroy();
                    SceneList.RemoveAt(CurrentSceneIndex);
                }

                SceneList.Add(new Scene(savePath));
                SortScenes(CurrentSortMode);
            }

            Busy = false;
            yield break;
        }

        public class Scene
        {
            public const int initialNumberOfMaids = -2;
            public Texture2D Thumbnail { get; }
            public FileInfo FileInfo { get; set; }
            public int NumberOfMaids { get; private set; } = initialNumberOfMaids;

            public Scene(string filePath)
            {
                FileInfo = new FileInfo(filePath);
                Thumbnail = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                Thumbnail.LoadImage(File.ReadAllBytes(FileInfo.FullName));
            }

            public void GetNumberOfMaids()
            {
                if (NumberOfMaids != initialNumberOfMaids) return;

                string filePath = FileInfo.FullName;

                byte[] sceneData = MeidoPhotoStudio.DecompressScene(filePath);

                if (sceneData == null) return;

                using (MemoryStream memoryStream = new MemoryStream(sceneData))
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, System.Text.Encoding.UTF8))
                {
                    try
                    {
                        if (binaryReader.ReadString() != "MPS_SCENE")
                        {
                            Utility.LogWarning($"'{filePath}' is not a {MeidoPhotoStudio.pluginName} scene");
                            return;
                        }

                        if (binaryReader.ReadInt32() > MeidoPhotoStudio.sceneVersion)
                        {
                            Utility.LogWarning(
                                $"'{filePath}' is made in a newer version of {MeidoPhotoStudio.pluginName}"
                            );
                            return;
                        }

                        NumberOfMaids = binaryReader.ReadInt32();
                    }
                    catch (Exception e)
                    {
                        Utility.LogWarning($"Failed to deserialize scene '{filePath}' because {e.Message}");
                        return;
                    }
                }
            }

            public void Destroy()
            {
                if (Thumbnail != null) UnityEngine.Object.DestroyImmediate(Thumbnail);
            }
        }
    }
}
