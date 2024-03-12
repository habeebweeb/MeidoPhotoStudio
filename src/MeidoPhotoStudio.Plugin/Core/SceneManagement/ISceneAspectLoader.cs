namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public interface ISceneAspectLoader<T>
{
    public void Load(T sceneAspectSchema, LoadOptions loadOptions);
}
