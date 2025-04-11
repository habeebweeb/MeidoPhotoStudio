namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LoadOption : IEnumerable<LoadOption>
{
    private readonly List<LoadOption> subOptionsList;
    private readonly Dictionary<string, LoadOption> subOptions;

    public LoadOption(string tag, bool enabled, params LoadOption[] subOptions)
    {
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag));

        Tag = tag;
        Enabled = enabled;

        _ = subOptions ?? throw new ArgumentNullException(nameof(subOptions));

        subOptionsList = new(subOptions.Length);
        this.subOptions = new(StringComparer.Ordinal);

        foreach (var subOption in subOptions)
        {
            _ = subOption ?? throw new InvalidLoadOptionException("Sub load options cannot be null.");

            if (this.subOptions.ContainsKey(subOption.Tag))
                throw new InvalidLoadOptionException("Duplicate sub load options are not allowed.");

            subOptionsList.Add(subOption);
            this.subOptions[subOption.Tag] = subOption;
        }
    }

    public string Tag { get; }

    public bool Enabled { get; set; }

    public LoadOption this[string tag] =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : subOptions[tag];

    public static LoadOption CreateFromSpec(LoadOptionSpec loadOptionSpec)
    {
        _ = loadOptionSpec ?? throw new ArgumentNullException(nameof(loadOptionSpec));

        var loadOption = new LoadOption(loadOptionSpec.Tag, loadOptionSpec.DefaultState);

        foreach (var subSpec in loadOptionSpec)
        {
            var subOption = new LoadOption(subSpec.Tag, subSpec.DefaultState);

            AddLoadOptions(subSpec, subOption);

            loadOption.subOptionsList.Add(subOption);
            loadOption.subOptions.Add(subOption.Tag, subOption);
        }

        static void AddLoadOptions(LoadOptionSpec loadOptionSpec, LoadOption loadOption)
        {
            foreach (var subSpec in loadOptionSpec)
            {
                var subLoadOption = new LoadOption(subSpec.Tag, subSpec.DefaultState);

                AddLoadOptions(subSpec, subLoadOption);

                loadOption.subOptionsList.Add(subLoadOption);
                loadOption.subOptions.Add(subLoadOption.Tag, subLoadOption);
            }
        }

        return loadOption;
    }

    public bool TryGetSubOption(string tag, out LoadOption subOption) =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : subOptions.TryGetValue(tag, out subOption);

    public bool SubOptionEnabled(string tag) =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : TryGetSubOption(tag, out var option) && option.Enabled;

    public IEnumerator<LoadOption> GetEnumerator() =>
        subOptionsList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
