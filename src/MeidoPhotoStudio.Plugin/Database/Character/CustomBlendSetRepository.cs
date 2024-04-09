using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Database.Character;

public class CustomBlendSetRepository(string blendSetsPath) : IEnumerable<CustomBlendSetModel>
{
    private readonly string blendSetsPath =
        string.IsNullOrEmpty(blendSetsPath)
            ? throw new ArgumentException($"'{nameof(blendSetsPath)}' cannot be null or empty.", nameof(blendSetsPath))
            : blendSetsPath;

    private Dictionary<string, List<CustomBlendSetModel>> blendSets;

    public event EventHandler<AddedBlendSetEventArgs> AddedBlendSet;

    public event EventHandler Refreshing;

    public event EventHandler Refreshed;

    public string RootCategoryName { get; } = "root";

    public IEnumerable<string> Categories =>
        BlendSets.Keys;

    private Dictionary<string, List<CustomBlendSetModel>> BlendSets =>
        blendSets ??= Initialize(blendSetsPath, RootCategoryName);

    public IList<CustomBlendSetModel> this[string category] =>
        BlendSets[category].AsReadOnly();

    public bool ContainsCategory(string category) =>
        BlendSets.ContainsKey(category);

    public CustomBlendSetModel GetByID(long id) =>
        this.FirstOrDefault(blendSet => blendSet.ID == id);

    public void Refresh()
    {
        Refreshing?.Invoke(this, EventArgs.Empty);

        blendSets = Initialize(blendSetsPath, RootCategoryName);

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerator<CustomBlendSetModel> GetEnumerator() =>
        BlendSets.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(FacialExpressionSet facialExpression, string category, string name)
    {
        _ = facialExpression ?? throw new ArgumentNullException(nameof(facialExpression));

        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        var directory = SanitizeFilename(category);
        var filename = SanitizeFilename(name);

        var fullDirectory = Path.Combine(blendSetsPath, directory);

        if (string.Equals(directory, RootCategoryName, StringComparison.Ordinal))
            fullDirectory = blendSetsPath;

        var fullPath = Path.Combine(fullDirectory, filename + ".xml");

        if (File.Exists(fullPath))
            fullPath = Path.Combine(fullDirectory, $"{filename}_{DateTime.Now:yyyyMMddHHmmss}.xml");

        var file = new FileInfo(fullPath);

        try
        {
            var checksum = WriteBlendSetData(facialExpression, file);

            if (!BlendSets.TryGetValue(directory, out var list))
                list = BlendSets[directory] = [];

            var customBlendSet = new CustomBlendSetModel(checksum, directory, fullPath);

            list.Add(customBlendSet);

            AddedBlendSet?.Invoke(this, new(customBlendSet));

            static long WriteBlendSetData(FacialExpressionSet facialExpression, FileInfo file)
            {
                file.Directory.Create();

                using var fileStream = File.Open(file.FullName, FileMode.CreateNew);

                new BlendSetSerializer().Serialize(facialExpression, fileStream);

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

    private static Dictionary<string, List<CustomBlendSetModel>> Initialize(
        string blendSetsPath, string rootCategoryName)
    {
        var blendSets = new Dictionary<string, List<CustomBlendSetModel>>(StringComparer.Ordinal);
        var blendSetsDirectory = new DirectoryInfo(blendSetsPath);
        var crc32 = new wf.CRC32();

        blendSetsDirectory.Create();

        blendSets[rootCategoryName] = [];

        GetBlendSets(rootCategoryName, blendSetsDirectory, blendSets[rootCategoryName]);

        foreach (var directory in blendSetsDirectory.GetDirectories())
        {
            blendSets[directory.Name] = [];

            GetBlendSets(directory.Name, directory, blendSets[directory.Name]);
        }

        return blendSets;

        void GetBlendSets(string categoryName, DirectoryInfo directory, List<CustomBlendSetModel> blendSetList) =>
            blendSetList.AddRange(directory.GetFiles("*.xml")
                .Select(file => CreateCustomBlendSet(categoryName, file))
                .Where(model => model is not null));

        CustomBlendSetModel CreateCustomBlendSet(string category, FileInfo file)
        {
            try
            {
                using var fileStream = file.OpenRead();

                var id = (long)uint.MaxValue + crc32.ComputeChecksum(fileStream);

                return new(id, category, file.FullName);
            }
            catch
            {
                return null;
            }
        }
    }
}
