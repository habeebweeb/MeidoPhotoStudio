using MeidoPhotoStudio.Plugin.Core.Extension;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Serialization;

namespace MeidoPhotoStudio.Plugin.Api.Extension;

public class ExtensionApi(
    ExtensionSchemaBuilder extensionSchemaBuilder,
    ExtensionAspectLoader extensionAspectLoader,
    ExtensionDataConverter extensionDataConverter)
{
    private readonly Dictionary<string, IExtensionSceneAspectController> sceneAspectExtensionControllers = [];

    private readonly ExtensionSchemaBuilder extensionSchemaBuilder = extensionSchemaBuilder
        ?? throw new ArgumentNullException(nameof(extensionSchemaBuilder));

    private readonly ExtensionAspectLoader extensionAspectLoader = extensionAspectLoader
        ?? throw new ArgumentNullException(nameof(extensionAspectLoader));

    private readonly ExtensionDataConverter extensionDataConverter = extensionDataConverter
        ?? throw new ArgumentNullException(nameof(extensionDataConverter));

    public void RegisterSceneAspectExtensionController(
        string extensionID, IExtensionSceneAspectController sceneAspectController)
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

        sceneAspectExtensionControllers[extensionID] = sceneAspectController;
    }

    public void DeregisterSceneAspectExtensionController(string extensionID)
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
        sceneAspectExtensionControllers.Remove(extensionID);
    }
}
