namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LoadOptions : ILoadOptions
{
    private readonly List<LoadOption> optionsList;
    private readonly Dictionary<string, LoadOption> options;

    public LoadOptions(params LoadOption[] options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        optionsList = new(options.Length);
        this.options = new(StringComparer.Ordinal);

        foreach (var option in options)
        {
            _ = option ?? throw new InvalidLoadOptionException("Load options cannot be null");

            if (this.options.ContainsKey(option.Tag))
                throw new InvalidLoadOptionException("Duplicate load options are not allowed.");

            optionsList.Add(option);
            this.options[option.Tag] = option;
        }
    }

    public event EventHandler<LoadOptionsChangedEventArgs> AddedOption;

    public event EventHandler<LoadOptionsChangedEventArgs> RemovedOption;

    public LoadOption this[string tag] =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : options[tag];

    public bool TryGetOption(string tag, out LoadOption option) =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : options.TryGetValue(tag, out option);

    public bool OptionEnabled(string tag) =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : TryGetOption(tag, out var option) && option.Enabled;

    public void AddLoadOption(LoadOption loadOption)
    {
        _ = loadOption ?? throw new ArgumentNullException(nameof(loadOption));

        if (options.ContainsKey(loadOption.Tag))
        {
            Plugin.Logger.LogInfo($"A load option with the tag '{loadOption.Tag}' is already registered.");

            return;
        }

        options.Add(loadOption.Tag, loadOption);
        optionsList.Add(loadOption);

        AddedOption?.Invoke(this, new(loadOption));
    }

    public void RemoveLoadOption(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag));

        if (!options.TryGetValue(tag, out var existingOption))
        {
            Plugin.Logger.LogInfo($"No load option with tag '{tag}' is registered.");

            return;
        }

        options.Remove(tag);
        optionsList.Remove(existingOption);

        RemovedOption?.Invoke(this, new(existingOption));
    }

    public IEnumerator<LoadOption> GetEnumerator() =>
        optionsList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
