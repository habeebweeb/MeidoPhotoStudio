namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LoadOptionsService
{
    private readonly List<LoadOptions> loadOptions = [];
    private readonly Dictionary<string, LoadOptionSpec> extensionLoadOptionSpecs = new(StringComparer.Ordinal);

    public ILoadOptions CreateLoadOptions()
    {
        var loadOptions = new LoadOptions(
        [
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
            new("props", true),
            .. extensionLoadOptionSpecs.Values.Select(LoadOption.CreateFromSpec)
        ]);

        this.loadOptions.Add(loadOptions);

        return loadOptions;
    }

    public void RegisterExtensionLoadOption(LoadOptionSpec loadOptionSpec)
    {
        _ = loadOptionSpec ?? throw new ArgumentNullException(nameof(loadOptionSpec));

        if (extensionLoadOptionSpecs.ContainsKey(loadOptionSpec.Tag))
        {
            Plugin.Logger.LogInfo($"A load option specification with tag '{loadOptionSpec.Tag}' is already registered.");

            return;
        }

        extensionLoadOptionSpecs.Add(loadOptionSpec.Tag, loadOptionSpec);

        foreach (var loadOptions in loadOptions)
            loadOptions.AddLoadOption(LoadOption.CreateFromSpec(loadOptionSpec));
    }

    public void DeregisterExtensionLoadOption(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag));

        if (!extensionLoadOptionSpecs.ContainsKey(tag))
        {
            Plugin.Logger.LogInfo($"No load option specification with tag '{tag}' is registered.");

            return;
        }

        extensionLoadOptionSpecs.Remove(tag);

        foreach (var loadOptions in loadOptions)
            loadOptions.RemoveLoadOption(tag);
    }
}
