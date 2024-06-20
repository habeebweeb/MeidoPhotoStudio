using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Serialization;

namespace MeidoPhotoStudio.Plugin.Core.Scenes;

public class QuickSaveService(
    string quickSaveDirectory,
    CharacterService characterService,
    SceneSchemaBuilder sceneSchemaBuilder,
    ISceneSerializer sceneSerializer,
    SceneLoader sceneLoader)
{
    private readonly string quickSaveDirectory = string.IsNullOrEmpty(quickSaveDirectory)
        ? throw new ArgumentException($"'{nameof(quickSaveDirectory)}' cannot be null", nameof(quickSaveDirectory))
        : quickSaveDirectory;

    private readonly CharacterService characterService = characterService
        ?? throw new ArgumentNullException(nameof(characterService));

    private readonly SceneSchemaBuilder sceneSchemaBuilder = sceneSchemaBuilder
        ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));

    private readonly ISceneSerializer sceneSerializer = sceneSerializer
        ?? throw new ArgumentNullException(nameof(sceneSerializer));

    private readonly SceneLoader sceneLoader = sceneLoader
        ?? throw new ArgumentNullException(nameof(sceneLoader));

    private string QuickSavePath =>
        Path.Combine(quickSaveDirectory, "mpsquicksave");

    public void QuickSave()
    {
        if (characterService.Busy)
            return;

        Directory.CreateDirectory(quickSaveDirectory);

        using var fileStream = File.OpenWrite(QuickSavePath);

        sceneSerializer.SerializeScene(fileStream, sceneSchemaBuilder.Build());
    }

    public void QuickLoad(LoadOptions? loadOptions = null)
    {
        if (characterService.Busy)
            return;

        try
        {
            using var fileStream = File.OpenRead(QuickSavePath);

            var scene = sceneSerializer.DeserializeScene(fileStream);

            if (scene is null)
                return;

            sceneLoader.LoadScene(scene, loadOptions ?? LoadOptions.All);
        }
        catch (IOException e)
        {
            Utility.LogDebug($"Could not load quick save file because {e}");
        }
        catch (Exception e)
        {
            Utility.LogDebug($"Could not load quick save because {e}");
        }
    }
}
