using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Framework.Service;

public class CustomMaidSceneService
{
    public CustomMaidSceneService()
    {
        UpdateCurrentScene(SceneManager.GetActiveScene());

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public enum CustomMaidScene
    {
        None,
        Office,
        Edit,
    }

    public bool EditScene =>
        CurrentScene is CustomMaidScene.Edit;

    public bool OfficeScene =>
        CurrentScene is CustomMaidScene.Office;

    public bool ValidScene =>
        CurrentScene is not CustomMaidScene.None;

    public CustomMaidScene CurrentScene { get; private set; }

    private static CustomMaidScene ConvertScene(Scene scene) =>
        scene.buildIndex switch
        {
            3 => CustomMaidScene.Office,
            5 => CustomMaidScene.Edit,
            _ => CustomMaidScene.None,
        };

    private void UpdateCurrentScene(Scene scene) =>
        CurrentScene = ConvertScene(scene);

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) =>
        UpdateCurrentScene(scene);
}
