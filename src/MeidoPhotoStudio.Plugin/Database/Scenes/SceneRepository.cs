using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Serialization;

namespace MeidoPhotoStudio.Database.Scenes;

public class SceneRepository(string scenesPath, ISceneSerializer sceneSerializer) : IEnumerable<SceneModel>
{
    private readonly string scenesPath =
        string.IsNullOrEmpty(scenesPath)
            ? throw new ArgumentNullException($"'{nameof(scenesPath)}' cannot be null or empty.", nameof(scenesPath))
            : scenesPath;

    private readonly ISceneSerializer sceneSerializer = sceneSerializer
        ?? throw new ArgumentNullException(nameof(sceneSerializer));

    private Dictionary<string, List<SceneModel>> scenes;

    public event EventHandler<SceneChangeEventArgs> AddedScene;

    public event EventHandler<SceneChangeEventArgs> RemovedScene;

    public event EventHandler<CategoryChangeEventArgs> AddedCategory;

    public event EventHandler<CategoryChangeEventArgs> RemovedCategory;

    public event EventHandler Refreshing;

    public event EventHandler Refreshed;

    public string RootCategoryName { get; } = "root";

    public IEnumerable<string> Categories =>
        Scenes.Keys;

    private Dictionary<string, List<SceneModel>> Scenes =>
        scenes ??= Initialize(scenesPath, RootCategoryName);

    public IList<SceneModel> this[string category] =>
        Scenes[category].AsReadOnly();

    public bool ContainsCategory(string category) =>
        Scenes.ContainsKey(category);

    public void Refresh()
    {
        Refreshing?.Invoke(this, EventArgs.Empty);

        foreach (var scene in scenes?.Values.SelectMany(list => list) ?? [])
            scene.DestroyThumnail();

        scenes = Initialize(scenesPath, RootCategoryName);

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerator<SceneModel> GetEnumerator() =>
        Scenes.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(SceneSchema scene, Texture2D screenshot, string category, string name)
    {
        _ = scene ?? throw new ArgumentNullException(nameof(scene));

        if (!screenshot)
            throw new ArgumentNullException(nameof(screenshot));

        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        var directory = SanitizeFilename(category);
        var filename = SanitizeFilename(name);

        var fullDirectory = Path.Combine(scenesPath, directory);

        if (string.Equals(directory, RootCategoryName, StringComparison.Ordinal))
            fullDirectory = scenesPath;

        var fullPath = Path.Combine(fullDirectory, filename + ".png");

        if (File.Exists(fullPath))
            fullPath = Path.Combine(fullDirectory, $"{filename}{DateTime.Now:yyyyMMddHHmmss}.png");

        var file = new FileInfo(fullPath);

        WriteSceneToFile(scene, screenshot, new(fullPath), false);

        var sceneModel = new SceneModel(directory, file.FullName);

        if (!Scenes.TryGetValue(directory, out var list))
        {
            list = Scenes[directory] = [];

            AddedCategory?.Invoke(this, new(directory));
        }

        list.Add(sceneModel);

        AddedScene?.Invoke(this, new(sceneModel));
    }

    public void Overwrite(SceneSchema scene, Texture2D screenshot, SceneModel overwritingModel)
    {
        _ = scene ?? throw new ArgumentNullException(nameof(scene));

        if (!screenshot)
            throw new ArgumentNullException(nameof(screenshot));

        _ = overwritingModel ?? throw new ArgumentNullException(nameof(overwritingModel));

        WriteSceneToFile(scene, screenshot, new(overwritingModel.Filename), true);

        var newModel = new SceneModel(overwritingModel.Category, overwritingModel.Filename);

        if (!Scenes.TryGetValue(overwritingModel.Category, out var list))
        {
            Scenes[overwritingModel.Category] = [newModel];

            AddedCategory?.Invoke(this, new(overwritingModel.Category));
        }
        else
        {
            var overwritingModelIndex = list.IndexOf(overwritingModel);

            if (overwritingModelIndex is not -1)
            {
                list.RemoveAt(overwritingModelIndex);
                list.Insert(overwritingModelIndex, newModel);

                RemovedScene?.Invoke(this, new(overwritingModel));
            }
            else
            {
                list.Add(newModel);
            }
        }

        overwritingModel.DestroyThumnail();

        AddedScene?.Invoke(this, new(newModel));
    }

    public void AddCategory(string category)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty", nameof(category));

        var sanitizedCategory = SanitizeFilename(category);

        if (string.Equals(sanitizedCategory, RootCategoryName, StringComparison.Ordinal))
            return;

        var fullPath = Path.Combine(scenesPath, sanitizedCategory);

        try
        {
            Directory.CreateDirectory(fullPath);
        }
        catch
        {
            // TODO: Log cannot create new category
            return;
        }

        if (ContainsCategory(sanitizedCategory))
            return;

        Scenes[sanitizedCategory] = [];

        AddedCategory?.Invoke(this, new(sanitizedCategory));
    }

    public void DeleteCategory(string category)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty", nameof(category));

        if (!ContainsCategory(category))
            return;

        if (string.Equals(category, Path.GetDirectoryName(scenesPath)))
            DeleteRoot();
        else
            DeleteCategory(category);

        RemovedCategory?.Invoke(this, new(category));

        void DeleteRoot()
        {
            var category = Path.GetDirectoryName(scenesPath);

            foreach (var model in Scenes[category])
                model.DestroyThumnail();

            Scenes[category] = [];

            foreach (var file in Directory.GetFiles(scenesPath, "*.png"))
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
        }

        void DeleteCategory(string category)
        {
            foreach (var model in Scenes[category])
                model.DestroyThumnail();

            Scenes.Remove(category);

            try
            {
                var directory = Path.Combine(scenesPath, category);

                Directory.Delete(directory, true);
            }
            catch
            {
                // TODO: Log directory deletion exception
            }
        }
    }

    public void Delete(SceneModel scene)
    {
        _ = scene ?? throw new ArgumentNullException(nameof(scene));

        if (!Scenes.ContainsKey(scene.Category))
            return;

        var sceneIndex = Scenes[scene.Category].IndexOf(scene);

        if (sceneIndex is -1)
            return;

        Scenes[scene.Category].RemoveAt(sceneIndex);

        try
        {
            File.Delete(scene.Filename);
        }
        catch
        {
            // TODO: Log file deletion exception
        }

        RemovedScene?.Invoke(this, new(scene));
    }

    private static Dictionary<string, List<SceneModel>> Initialize(string scenesPath, string rootCategoryName)
    {
        var presets = new Dictionary<string, List<SceneModel>>(StringComparer.Ordinal);
        var presetsDirectory = new DirectoryInfo(scenesPath);

        presetsDirectory.Create();

        presets[rootCategoryName] = [];

        GetPresets(rootCategoryName, presetsDirectory, presets[rootCategoryName]);

        foreach (var directory in presetsDirectory.GetDirectories())
        {
            presets[directory.Name] = [];

            GetPresets(directory.Name, directory, presets[directory.Name]);
        }

        return presets;

        void GetPresets(string categoryName, DirectoryInfo directory, List<SceneModel> scenes) =>
            scenes.AddRange(directory.GetFiles("*.png")
                .Select(file => new SceneModel(categoryName, file.FullName))
                .Where(model => model is not null));
    }

    private static string SanitizeFilename(string filePath) =>
        string.Join("_", filePath.Trim().Split(Path.GetInvalidFileNameChars()))
            .Replace(".", string.Empty).Trim('_');

    private void WriteSceneToFile(SceneSchema scene, Texture2D screenshot, FileInfo file, bool overwrite)
    {
        try
        {
            file.Directory.Create();

            using var fileStream = File.Open(file.FullName, overwrite ? FileMode.Truncate : FileMode.CreateNew);

            ResizeToFit(screenshot, 480, 270);

            var encodedScreenshot = screenshot.EncodeToPNG();

            fileStream.Write(encodedScreenshot, 0, encodedScreenshot.Length);

            sceneSerializer.SerializeScene(fileStream, scene);
        }
        catch (Exception e) when (e is IOException)
        {
            return;
        }

        static void ResizeToFit(Texture2D texture, int maxWidth, int maxHeight)
        {
            var width = texture.width;
            var height = texture.height;

            if (width == maxWidth && height == maxHeight)
                return;

            var scale = Mathf.Min(maxWidth / (float)width, maxHeight / (float)height);

            width = Mathf.RoundToInt(width * scale);
            height = Mathf.RoundToInt(height * scale);
            TextureScale.Bilinear(texture, width, height);
        }
    }
}
