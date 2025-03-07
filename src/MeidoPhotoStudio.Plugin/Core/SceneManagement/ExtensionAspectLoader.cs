using MeidoPhotoStudio.Plugin.Core.Schema.Extension;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class ExtensionAspectLoader : ISceneAspectLoader<ExtensionSchema>
{
    private readonly Dictionary<string, IExtensionSceneAspectLoader> aspectLoaders = [];

    public bool RegisterAspectLoader(string extensionID, IExtensionSceneAspectLoader aspectLoader)
    {
        if (string.IsNullOrEmpty(extensionID))
            throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID));

        _ = aspectLoader ?? throw new ArgumentNullException(nameof(aspectLoader));

        if (aspectLoaders.ContainsKey(extensionID))
        {
            Plugin.Logger.LogWarning($"Extension aspect loader with ID '{extensionID}' is already registered.");

            return false;
        }

        aspectLoaders[extensionID] = aspectLoader;

        return true;
    }

    public bool DeregisterAspectLoader(string extensionID) =>
        string.IsNullOrEmpty(extensionID)
            ? throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID))
            : aspectLoaders.Remove(extensionID);

    public void Load(ExtensionSchema sceneAspectSchema, LoadOptions loadOptions)
    {
        foreach (var data in sceneAspectSchema.ExtensionData)
        {
            if (data is null)
                continue;

            if (string.IsNullOrEmpty(data.ExtensionID))
                continue;

            if (!aspectLoaders.TryGetValue(data.ExtensionID, out var aspectLoader))
                continue;

            try
            {
                aspectLoader.Load(data, loadOptions);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Extension aspect loader with ID '{data.ExtensionID}' threw unhandled exception.\n{e}");
            }
        }
    }
}
