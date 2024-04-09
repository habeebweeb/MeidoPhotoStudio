using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Database.Character;

public class CustomAnimationRepository(string customAnimationsPath) : IEnumerable<CustomAnimationModel>
{
    private readonly string customAnimationsPath = string.IsNullOrEmpty(customAnimationsPath)
        ? throw new ArgumentException($"'{nameof(customAnimationsPath)}' cannot be null or empty.", nameof(customAnimationsPath))
        : customAnimationsPath;

    private Dictionary<string, List<CustomAnimationModel>> poses;

    public event EventHandler<AddedAnimationEventArgs> AddedAnimation;

    public event EventHandler Refreshing;

    public event EventHandler Refreshed;

    public string RootCategoryName { get; } = "root";

    public IEnumerable<string> Categories =>
        Animations.Keys;

    private Dictionary<string, List<CustomAnimationModel>> Animations =>
        poses ??= InitializeAnimations(customAnimationsPath, RootCategoryName);

    public IList<CustomAnimationModel> this[string category] =>
        Animations[category];

    public bool ContainsCategory(string category) =>
        Animations.ContainsKey(category);

    public CustomAnimationModel GetByID(long id) =>
        this.FirstOrDefault(pose => pose.ID == id);

    public void Refresh()
    {
        Refreshing?.Invoke(this, EventArgs.Empty);

        poses = InitializeAnimations(customAnimationsPath, RootCategoryName);

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerator<CustomAnimationModel> GetEnumerator() =>
        Animations.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(byte[] animationData, string category, string name)
    {
        _ = animationData ?? throw new ArgumentNullException(nameof(animationData));

        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        var directory = SanitizeFilename(category);
        var filename = SanitizeFilename(name);

        var fullDirectory = Path.Combine(customAnimationsPath, directory);

        if (string.Equals(directory, RootCategoryName, StringComparison.Ordinal))
            fullDirectory = customAnimationsPath;

        var fullPath = Path.Combine(fullDirectory, filename + ".anm");

        if (File.Exists(fullPath))
            fullPath = Path.Combine(fullDirectory, $"{filename}_{DateTime.Now:yyyyMMddHHmmss}.xml");

        var file = new FileInfo(fullPath);

        try
        {
            var checksum = WriteAnimationData(animationData, file);

            if (!Animations.TryGetValue(directory, out var list))
                list = Animations[directory] = [];

            var customAnimation = new CustomAnimationModel(checksum, directory, file.FullName);

            list.Add(customAnimation);

            AddedAnimation?.Invoke(this, new(customAnimation));

            static long WriteAnimationData(byte[] animationData, FileInfo file)
            {
                file.Directory.Create();

                using var fileStream = File.Open(file.FullName, FileMode.CreateNew);

                fileStream.Write(animationData, 0, animationData.Length);

                fileStream.Position = 0;

                return (long)uint.MaxValue + new wf.CRC32().ComputeChecksum(fileStream);
            }
        }
        catch (Exception e) when (e is IOException)
        {
            return;
        }

        static string SanitizeFilename(string filePath) =>
            string.Join("_", filePath.Trim().Split(Path.GetInvalidFileNameChars()))
                .Replace(".", string.Empty).Trim('_');
    }

    private Dictionary<string, List<CustomAnimationModel>> InitializeAnimations(
        string customAnimationsPath, string rootCategoryName)
    {
        var animations = new Dictionary<string, List<CustomAnimationModel>>(StringComparer.Ordinal);
        var animationsDirectory = new DirectoryInfo(customAnimationsPath);
        var crc32 = new wf.CRC32();

        animationsDirectory.Create();

        animations[rootCategoryName] = [];

        GetAnimations(rootCategoryName, animationsDirectory, animations[rootCategoryName]);

        foreach (var directory in animationsDirectory.GetDirectories())
        {
            animations[directory.Name] = [];

            GetAnimations(directory.Name, directory, animations[directory.Name]);
        }

        return animations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        void GetAnimations(string categoryName, DirectoryInfo directory, List<CustomAnimationModel> poseList) =>
            poseList.AddRange(directory.GetFiles("*.anm")
                .Select(file => CreateAnimation(categoryName, file))
                .Where(model => model is not null));

        CustomAnimationModel CreateAnimation(string category, FileInfo file)
        {
            try
            {
                var data = file.OpenRead();
                var id = (long)uint.MaxValue + crc32.ComputeChecksum(data);

                return new(id, category, file.FullName);
            }
            catch
            {
                return null;
            }
        }
    }
}
