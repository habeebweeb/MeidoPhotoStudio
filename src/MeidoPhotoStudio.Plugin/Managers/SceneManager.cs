using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BepInEx.Configuration;
using UnityEngine;

using Input = MeidoPhotoStudio.Plugin.InputManager;
using Object = UnityEngine.Object;

namespace MeidoPhotoStudio.Plugin;

public class SceneManager : IManager
{
    public static readonly Vector2 SceneDimensions = new(480, 270);

    private static readonly ConfigEntry<bool> SortDescendingConfig =
        Configuration.Config.Bind("SceneManager", "SortDescending", false, "Sort scenes descending (Z-A)");

    private static readonly ConfigEntry<SortMode> CurrentSortModeConfig =
        Configuration.Config.Bind("SceneManager", "SortMode", SortMode.Name, "Scene sorting mode");

    private static byte[] tempSceneData;

    private readonly ScreenshotService screenshotService;
    private readonly SceneSerializer sceneSerializer;

    static SceneManager()
    {
        Input.Register(MpsKey.OpenSceneManager, KeyCode.F8, "Hide/show scene manager");
        Input.Register(MpsKey.SaveScene, KeyCode.S, "Quick save scene");
        Input.Register(MpsKey.LoadScene, KeyCode.A, "Load quick saved scene");
    }

    public SceneManager(ScreenshotService screenshotService, SceneSerializer sceneSerializer)
    {
        this.screenshotService = screenshotService;
        this.sceneSerializer = sceneSerializer;

        Activate();
    }

    public enum SortMode
    {
        Name,
        DateCreated,
        DateModified,
    }

    public static bool Busy { get; private set; }

    public bool Initialized { get; private set; }

    public bool KankyoMode { get; set; }

    public int CurrentSceneIndex { get; private set; } = -1;

    public List<MPSScene> SceneList { get; } = new();

    public int CurrentDirectoryIndex { get; private set; } = -1;

    public bool SortDescending
    {
        get => SortDescendingConfig.Value;
        set => SortDescendingConfig.Value = value;
    }

    public string CurrentDirectoryName =>
        CurrentDirectoryList[CurrentDirectoryIndex];

    public List<string> CurrentDirectoryList =>
        KankyoMode ? Constants.KankyoDirectoryList : Constants.SceneDirectoryList;

    public string CurrentBasePath =>
        KankyoMode ? Constants.KankyoPath : Constants.ScenesPath;

    public string CurrentScenesDirectory =>
        CurrentDirectoryIndex is 0 ? CurrentBasePath : Path.Combine(CurrentBasePath, CurrentDirectoryName);

    public SortMode CurrentSortMode
    {
        get => CurrentSortModeConfig.Value;
        private set => CurrentSortModeConfig.Value = value;
    }

    public MPSScene CurrentScene =>
        SceneList.Count is 0 ? null : SceneList[CurrentSceneIndex];

    private static string TempScenePath =>
        Path.Combine(Constants.ConfigPath, "mpstempscene");

    private int SortDirection =>
        SortDescending ? -1 : 1;

    public void Activate()
    {
    }

    public void Initialize()
    {
        if (Initialized)
            return;

        Initialized = true;
        SelectDirectory(0);
    }

    public void Deactivate() =>
        ClearSceneList();

    public void Update()
    {
        if (!Input.Control)
            return;

        if (Input.GetKeyDown(MpsKey.SaveScene))
            QuickSaveScene();
        else if (Input.GetKeyDown(MpsKey.LoadScene))
            QuickLoadScene();
    }

    public void DeleteDirectory()
    {
        if (Directory.Exists(CurrentScenesDirectory))
            Directory.Delete(CurrentScenesDirectory, true);

        CurrentDirectoryList.RemoveAt(CurrentDirectoryIndex);
        CurrentDirectoryIndex = Mathf.Clamp(CurrentDirectoryIndex, 0, CurrentDirectoryList.Count - 1);

        UpdateSceneList();
    }

    public void OverwriteScene() =>
        SaveScene(overwrite: true);

    public void ToggleKankyoMode()
    {
        KankyoMode = !KankyoMode;
        CurrentDirectoryIndex = 0;

        UpdateSceneList();
    }

    public void SaveScene(bool overwrite = false)
    {
        if (Busy)
            return;

        Busy = true;

        if (!Directory.Exists(CurrentScenesDirectory))
            Directory.CreateDirectory(CurrentScenesDirectory);

        screenshotService.TakeScreenshotToTexture(SaveScene);

        void SaveScene(Texture2D screenshot) =>
            SaveSceneToFile(screenshot, overwrite);
    }

    public void SelectDirectory(int directoryIndex)
    {
        directoryIndex = Mathf.Clamp(directoryIndex, 0, CurrentDirectoryList.Count - 1);

        if (directoryIndex == CurrentDirectoryIndex)
            return;

        CurrentDirectoryIndex = directoryIndex;

        UpdateSceneList();
    }

    public void SelectScene(int sceneIndex)
    {
        CurrentSceneIndex = Mathf.Clamp(sceneIndex, 0, SceneList.Count - 1);
        CurrentScene.Preload();
    }

    public void AddDirectory(string directoryName)
    {
        directoryName = Utility.SanitizePathPortion(directoryName);

        if (CurrentDirectoryList.Contains(directoryName, StringComparer.InvariantCultureIgnoreCase))
            return;

        var finalPath = Path.Combine(CurrentBasePath, directoryName);
        var fullPath = Path.GetFullPath(finalPath);

        if (!fullPath.StartsWith(CurrentBasePath))
        {
            var baseDirectoryName = KankyoMode ? Constants.KankyoDirectory : Constants.SceneDirectory;

            Utility.LogError($"Could not add directory to {baseDirectoryName}. Path is invalid: '{fullPath}'");

            return;
        }

        CurrentDirectoryList.Add(directoryName);
        Directory.CreateDirectory(finalPath);

        UpdateDirectoryList();
        CurrentDirectoryIndex = CurrentDirectoryList.IndexOf(directoryName);

        UpdateSceneList();
    }

    public void Refresh()
    {
        if (!Directory.Exists(CurrentScenesDirectory))
            CurrentDirectoryIndex = 0;

        if (KankyoMode)
            Constants.InitializeKankyoDirectories();
        else
            Constants.InitializeSceneDirectories();

        UpdateSceneList();
    }

    public void SortScenes(SortMode sortMode)
    {
        CurrentSortMode = sortMode;

        Comparison<MPSScene> comparator = CurrentSortMode switch
        {
            SortMode.DateModified => SortByDateModified,
            SortMode.DateCreated => SortByDateCreated,
            SortMode.Name => SortByName,
            _ => SortByName,
        };

        SceneList.Sort(comparator);
    }

    public void DeleteScene()
    {
        if (CurrentScene.FileInfo.Exists)
            CurrentScene.FileInfo.Delete();

        SceneList.RemoveAt(CurrentSceneIndex);
        CurrentSceneIndex = Mathf.Clamp(CurrentSceneIndex, 0, SceneList.Count - 1);
    }

    public void LoadScene(MPSScene scene) =>
        sceneSerializer.DeserializeScene(scene.Data);

    private int SortByName(MPSScene a, MPSScene b) =>
        SortDirection * WindowsLogicalComparer.StrCmpLogicalW(a.FileInfo.Name, b.FileInfo.Name);

    private int SortByDateCreated(MPSScene a, MPSScene b) =>
        SortDirection * DateTime.Compare(a.FileInfo.CreationTime, b.FileInfo.CreationTime);

    private int SortByDateModified(MPSScene a, MPSScene b) =>
        SortDirection * DateTime.Compare(a.FileInfo.LastWriteTime, b.FileInfo.LastWriteTime);

    private void UpdateSceneList()
    {
        ClearSceneList();

        if (!Directory.Exists(CurrentScenesDirectory))
            Directory.CreateDirectory(CurrentScenesDirectory);

        foreach (var filename in Directory.GetFiles(CurrentScenesDirectory))
            if (Path.GetExtension(filename) is ".png")
                SceneList.Add(new(filename));

        SortScenes(CurrentSortMode);

        CurrentSceneIndex = Mathf.Clamp(CurrentSceneIndex, 0, SceneList.Count - 1);
    }

    private void UpdateDirectoryList()
    {
        var baseDirectoryName = KankyoMode ? Constants.KankyoDirectory : Constants.SceneDirectory;

        CurrentDirectoryList.Sort((a, b) =>
            a.Equals(baseDirectoryName, StringComparison.InvariantCultureIgnoreCase)
                ? -1
                : WindowsLogicalComparer.StrCmpLogicalW(a, b));
    }

    private void ClearSceneList()
    {
        foreach (var scene in SceneList)
            scene.Destroy();

        SceneList.Clear();
    }

    private void QuickSaveScene()
    {
        if (Busy)
            return;

        var data = sceneSerializer.SerializeScene();

        if (data is null)
            return;

        tempSceneData = data;

        File.WriteAllBytes(TempScenePath, data);
    }

    private void QuickLoadScene()
    {
        if (Busy)
            return;

        if (tempSceneData is null)
        {
            if (File.Exists(TempScenePath))
                tempSceneData = File.ReadAllBytes(TempScenePath);
            else
                return;
        }

        sceneSerializer.DeserializeScene(tempSceneData);
    }

    private void SaveSceneToFile(Texture2D screenshot, bool overwrite = false)
    {
        Busy = true;

        var sceneData = sceneSerializer.SerializeScene(KankyoMode);

        if (sceneData is not null)
        {
            var scenePrefix = KankyoMode ? "mpskankyo" : "mpsscene";
            var fileName = $"{scenePrefix}{Utility.Timestamp}.png";
            var savePath = Path.Combine(CurrentScenesDirectory, fileName);

            Utility.ResizeToFit(screenshot, (int)SceneDimensions.x, (int)SceneDimensions.y);

            try
            {
                if (overwrite && CurrentScene?.FileInfo is not null)
                    savePath = CurrentScene.FileInfo.FullName;
                else
                    overwrite = false;

                using (var fileStream = File.Create(savePath))
                {
                    var encodedPng = screenshot.EncodeToPNG();

                    fileStream.Write(encodedPng, 0, encodedPng.Length);
                    fileStream.Write(sceneData, 0, sceneData.Length);
                }

                if (overwrite)
                {
                    File.SetCreationTime(savePath, CurrentScene.FileInfo.CreationTime);
                    CurrentScene.Destroy();
                    SceneList.RemoveAt(CurrentSceneIndex);
                }
            }
            catch (Exception e)
            {
                Utility.LogError($"Failed to save scene to disk because {e.Message}\n{e.StackTrace}");
                Object.DestroyImmediate(screenshot);
                Busy = false;

                return;
            }

            SceneList.Add(new(savePath, screenshot));
            SortScenes(CurrentSortMode);
        }
        else
        {
            Object.DestroyImmediate(screenshot);
        }

        Busy = false;
    }
}
