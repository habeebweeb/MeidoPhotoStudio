using System.Text.RegularExpressions;

using MeidoPhotoStudio.Plugin.Core.Extension;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Serialization;

namespace MeidoPhotoStudio.Plugin.Api.Extension;

public class ExtensionApi(
    ExtensionSchemaBuilder extensionSchemaBuilder,
    ExtensionAspectLoader extensionAspectLoader,
    ExtensionDataConverter extensionDataConverter,
    LoadOptionsService loadOptionsService)
{
    private static readonly Regex AllowedGuidRegex = new(@"^[a-zA-Z0-9\._\-]+$");

    private readonly Dictionary<string, IExtensionSceneAspectController> sceneAspectExtensionControllers = [];
    private readonly Dictionary<string, Core.Extension.Extension> extensions = new(StringComparer.Ordinal);

    private readonly ExtensionSchemaBuilder extensionSchemaBuilder = extensionSchemaBuilder
        ?? throw new ArgumentNullException(nameof(extensionSchemaBuilder));

    private readonly ExtensionAspectLoader extensionAspectLoader = extensionAspectLoader
        ?? throw new ArgumentNullException(nameof(extensionAspectLoader));

    private readonly ExtensionDataConverter extensionDataConverter = extensionDataConverter
        ?? throw new ArgumentNullException(nameof(extensionDataConverter));

    private readonly LoadOptionsService loadOptionsService = loadOptionsService
        ?? throw new ArgumentNullException(nameof(loadOptionsService));

    public void RegisterExtension(Core.Extension.Extension extension)
    {
        if (extension == null)
            throw new ArgumentNullException(nameof(extension));

        if (string.IsNullOrEmpty(extension.GUID) || !AllowedGuidRegex.IsMatch(extension.GUID))
        {
            Plugin.Logger.LogWarning($"Extension GUID '{extension.GUID}' is not a valid GUID format.");

            return;
        }

        if (extensions.TryGetValue(extension.GUID, out var existingExtension))
        {
            Plugin.Logger.LogWarning($"An extension with GUID '{extension.GUID}' is already registered: '{existingExtension}'.");

            return;
        }

        extensions.Add(extension.GUID, extension);

        if (extension.SceneAspectController is ExtensionSceneAspectController sceneAspectController)
            RegisterSceneAspectExtensionController(extension.GUID, sceneAspectController);

        Plugin.Logger.LogInfo($"Registered extension [{extension.Name} ({extension.GUID})]");
    }

    public void DeregisterExtension(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            throw new ArgumentException($"'{nameof(guid)}' cannot be null or empty.", nameof(guid));

        if (!AllowedGuidRegex.IsMatch(guid))
        {
            Plugin.Logger.LogWarning($"Extension GUID '{guid}' is not a valid GUID format.");

            return;
        }

        if (!extensions.TryGetValue(guid, out var extension))
        {
            Plugin.Logger.LogWarning($"An extension with GUID '{guid}' is not registered.");

            return;
        }

        extensions.Remove(guid);

        if (extension.SceneAspectController is not null)
            DeregisterSceneAspectExtensionController(guid);

        Plugin.Logger.LogInfo($"Deregistered extension [{extension.Name} ({extension.GUID})]");
    }

    private void RegisterSceneAspectExtensionController(string extensionID, IExtensionSceneAspectController sceneAspectController)
    {
        if (string.IsNullOrEmpty(extensionID))
            throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID));

        _ = sceneAspectController ?? throw new ArgumentNullException(nameof(sceneAspectController));

        if (!typeof(IExtensionData).IsAssignableFrom(sceneAspectController.DataType))
            throw new InvalidExtensionDataTypeException($"Controller's data type is not assignable to '{typeof(IExtensionData)}'");

        if (sceneAspectExtensionControllers.ContainsKey(extensionID))
        {
            Plugin.Logger.LogInfo($"Scene aspect extension controller with ID '{extensionID}' is already registered.");

            return;
        }

        extensionDataConverter.RegisterExtensionType(extensionID, sceneAspectController.DataType);
        extensionSchemaBuilder.RegisterSchemaBuilder(extensionID, sceneAspectController);
        extensionAspectLoader.RegisterAspectLoader(extensionID, sceneAspectController);
        loadOptionsService.RegisterExtensionLoadOption(
            new(extensionID, true, [.. sceneAspectController.SubLoadOptions]));

        sceneAspectExtensionControllers[extensionID] = sceneAspectController;
    }

    private void DeregisterSceneAspectExtensionController(string extensionID)
    {
        if (string.IsNullOrEmpty(extensionID))
            throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID));

        if (!sceneAspectExtensionControllers.ContainsKey(extensionID))
        {
            Plugin.Logger.LogInfo($"No scene aspect extension controller with ID '{extensionID}' is registered.");

            return;
        }

        extensionDataConverter.DeregisterExtensionType(extensionID);
        extensionSchemaBuilder.DeregisterSchemaBuilder(extensionID);
        extensionAspectLoader.DeregisterAspectLoader(extensionID);
        loadOptionsService.DeregisterExtensionLoadOption(extensionID);

        sceneAspectExtensionControllers.Remove(extensionID);
    }
}
