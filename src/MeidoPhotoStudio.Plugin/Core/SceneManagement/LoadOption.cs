namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LoadOption : IEnumerable<LoadOption>
{
    private readonly LoadOption[] subOptionsList;
    private readonly Dictionary<string, LoadOption> subOptions;

    public LoadOption(string tag, bool enabled, params LoadOption[] subOptions)
    {
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag));

        Tag = tag;
        Enabled = enabled;

        _ = subOptions ?? throw new ArgumentNullException(nameof(subOptions));

        subOptionsList = new LoadOption[subOptions.Length];
        this.subOptions = new(StringComparer.Ordinal);

        for (var i = 0; i < subOptions.Length; i++)
        {
            var subOption = subOptions[i] ?? throw new InvalidLoadOptionException("Sub load options cannot be null.");

            if (this.subOptions.ContainsKey(subOption.Tag))
                throw new InvalidLoadOptionException("Duplicate sub load options are not allowed.");

            subOptionsList[i] = subOption;
            this.subOptions[subOption.Tag] = subOption;
        }
    }

    public string Tag { get; }

    public bool Enabled { get; set; }

    public LoadOption this[string tag] =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : subOptions[tag];

    public bool TryGetSubOption(string tag, out LoadOption subOption) =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : subOptions.TryGetValue(tag, out subOption);

    public bool SubOptionEnabled(string tag) =>
        string.IsNullOrEmpty(tag)
            ? throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag))
            : TryGetSubOption(tag, out var option) && option.Enabled;

    public IEnumerator<LoadOption> GetEnumerator() =>
        ((IEnumerable<LoadOption>)subOptionsList).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
