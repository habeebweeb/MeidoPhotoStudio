namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LoadOptions : IEnumerable<LoadOption>
{
    private readonly LoadOption[] optionsList;
    private readonly Dictionary<string, LoadOption> options;

    public LoadOptions(params LoadOption[] options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        optionsList = new LoadOption[options.Length];
        this.options = new(StringComparer.Ordinal);

        for (var i = 0; i < options.Length; i++)
        {
            var option = options[i] ?? throw new InvalidLoadOptionException("Load options cannot be null");

            if (this.options.ContainsKey(option.Tag))
                throw new InvalidLoadOptionException("Duplicate load options are not allowed.");

            optionsList[i] = option;
            this.options[option.Tag] = option;
        }
    }

    public static LoadOptions All =>
        new(
            new(
                "characters",
                true,
                new LoadOption("byID", false)),
            new("message", true),
            new("camera", true),
            new("lights", true),
            new(
                "effects",
                true,
                new("bloom", true),
                new("depthOfField", true),
                new("vignette", true),
                new("fog", true),
                new("sepiaTone", true),
                new("blur", true)),
            new("background", true),
            new("props", true));

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

    public IEnumerator<LoadOption> GetEnumerator() =>
        ((IEnumerable<LoadOption>)optionsList).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
