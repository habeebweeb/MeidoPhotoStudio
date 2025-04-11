namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LoadOptionSpec : IEnumerable<LoadOptionSpec>
{
    private readonly List<LoadOptionSpec> subLoadOptionsList = [];
    private readonly Dictionary<string, LoadOptionSpec> subOptions;

    public LoadOptionSpec(string tag, bool defaultState = true, params LoadOptionSpec[] subOptions)
    {
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag));

        Tag = tag;
        DefaultState = defaultState;

        _ = subOptions ?? throw new ArgumentNullException(nameof(subOptions));

        this.subOptions = new(StringComparer.Ordinal);
        subLoadOptionsList = [];

        foreach (var option in subOptions)
        {
            _ = option ?? throw new InvalidLoadOptionException("Load option specs cannot be null");

            if (this.subOptions.ContainsKey(option.Tag))
                throw new InvalidLoadOptionException("Duplicate load option specs are not allowed");

            subLoadOptionsList.Add(option);
            this.subOptions[option.Tag] = option;
        }
    }

    public string Tag { get; }

    public bool DefaultState { get; }

    public IEnumerator<LoadOptionSpec> GetEnumerator() =>
        subLoadOptionsList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
