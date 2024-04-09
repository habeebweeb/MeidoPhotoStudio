using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Database.Character;

public class HandPresetRepository(string presetsPath) : IEnumerable<HandPresetModel>
{
    private readonly string presetsPath =
        string.IsNullOrEmpty(presetsPath)
            ? throw new ArgumentNullException($"'{nameof(presetsPath)}' cannot be null or empty.", nameof(presetsPath))
            : presetsPath;

    private Dictionary<string, List<HandPresetModel>> presets;

    public event EventHandler<AddedHandPresetEventArgs> AddedHandPreset;

    public event EventHandler Refreshing;

    public event EventHandler Refreshed;

    public string RootCategoryName { get; } = "root";

    public IEnumerable<string> Categories =>
        Presets.Keys;

    private Dictionary<string, List<HandPresetModel>> Presets =>
        presets ??= Initialize(presetsPath, RootCategoryName);

    public IList<HandPresetModel> this[string category] =>
        Presets[category].AsReadOnly();

    public bool ContainsCategory(string category) =>
        Presets.ContainsKey(category);

    public HandPresetModel GetByID(long id) =>
        this.FirstOrDefault(preset => preset.ID == id);

    public void Refresh()
    {
        Refreshing?.Invoke(this, EventArgs.Empty);

        presets = Initialize(presetsPath, RootCategoryName);

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerator<HandPresetModel> GetEnumerator() =>
        Presets.Values.SelectMany(list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(HandOrFootPreset preset, string category, string name)
    {
        _ = preset ?? throw new ArgumentNullException(nameof(preset));

        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        var directory = SanitizeFilename(category);
        var filename = SanitizeFilename(name);

        var fullDirectory = Path.Combine(presetsPath, directory);

        if (string.Equals(directory, RootCategoryName, StringComparison.Ordinal))
            fullDirectory = presetsPath;

        var fullPath = Path.Combine(fullDirectory, filename + ".xml");

        if (File.Exists(fullPath))
            fullPath = Path.Combine(fullDirectory, $"{filename}_{DateTime.Now:yyyyMMddHHmmss}.xml");

        var file = new FileInfo(fullPath);

        try
        {
            var checksum = WritePresetData(preset, file);

            if (!Presets.TryGetValue(directory, out var list))
                list = Presets[directory] = [];

            var handPreset = new HandPresetModel(checksum, directory, fullPath);

            list.Add(handPreset);

            AddedHandPreset?.Invoke(this, new(handPreset));

            static long WritePresetData(HandOrFootPreset preset, FileInfo file)
            {
                file.Directory.Create();

                using var fileStream = File.Open(file.FullName, FileMode.CreateNew);

                new HandPresetSerializer().Serialize(preset, fileStream);

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

    private static Dictionary<string, List<HandPresetModel>> Initialize(string presetsPath, string rootCategoryName)
    {
        var presets = new Dictionary<string, List<HandPresetModel>>(StringComparer.Ordinal);
        var presetsDirectory = new DirectoryInfo(presetsPath);
        var crc32 = new wf.CRC32();

        presetsDirectory.Create();

        presets[rootCategoryName] = [];

        GetPresets(rootCategoryName, presetsDirectory, presets[rootCategoryName]);

        foreach (var directory in presetsDirectory.GetDirectories())
        {
            presets[directory.Name] = [];

            GetPresets(directory.Name, directory, presets[directory.Name]);
        }

        return presets;

        void GetPresets(string categoryName, DirectoryInfo directory, List<HandPresetModel> presetsList) =>
            presetsList.AddRange(directory.GetFiles("*.xml")
                .Select(file => CreateHandPresetModel(categoryName, file))
                .Where(model => model is not null));

        HandPresetModel CreateHandPresetModel(string category, FileInfo file)
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
