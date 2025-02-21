using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightService(TransformWatcher transformWatcher) : IEnumerable<LightController>, IIndexableCollection<LightController>, IActivateable
{
    private static GameObject lightParent;

    private readonly List<LightController> lightControllers = [];
    private readonly TransformWatcher transformWatcher =
        transformWatcher ? transformWatcher : throw new ArgumentNullException(nameof(transformWatcher));

    private LightProperties initialMainLightProperties;

    public event EventHandler<LightServiceEventArgs> AddedLight;

    public event EventHandler<LightServiceEventArgs> RemovingLight;

    public event EventHandler<LightServiceEventArgs> RemovedLight;

    public int Count =>
        lightControllers.Count;

    private static GameObject LightParent
    {
        get
        {
            if (lightParent)
                return lightParent;

            const string lightParentName = "[MPS Light Parent]";

            var foundParent = GameObject.Find(lightParentName);

            return lightParent = foundParent ? foundParent : new(lightParentName);
        }
    }

    public LightController this[int index] =>
        (uint)index >= lightControllers.Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : lightControllers[index];

    public void AddLight()
    {
        var lightGameObject = new GameObject("[MPS Light]");

        lightGameObject.transform.SetParent(LightParent.transform, false);

        var light = lightGameObject.AddComponent<Light>();

        AddLight(light);
    }

    public void AddLight(Light light)
    {
        var lightController = new LightController(light, transformWatcher);

        light.transform.position = LightController.DefaultPosition;

        if (IsMainLight(lightController))
        {
            BackupMainLight(lightController);
            ResetMainLight();
        }

        lightControllers.Add(lightController);

        AddedLight?.Invoke(this, new(lightController, lightControllers.Count - 1));
    }

    public int IndexOf(LightController lightController) =>
        lightController is null
            ? throw new ArgumentNullException(nameof(lightController))
            : lightControllers.IndexOf(lightController);

    public void RemoveLight(int index)
    {
        if ((uint)index >= lightControllers.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var lightController = lightControllers[index];

        if (IsMainLight(lightController))
        {
            RestoreMainLight(lightController);
            ResetMainLight();
        }

        RemovingLight?.Invoke(this, new(lightController, index));

        lightControllers.RemoveAt(index);

        RemovedLight?.Invoke(this, new(lightController, index));

        lightController.Destroy();

        if (!IsMainLight(lightController) && lightController.Light)
            Object.Destroy(lightController.Light.gameObject);
    }

    public void RemoveLight(LightController lightController)
    {
        var lightIndex = lightControllers.IndexOf(lightController);

        if (lightIndex is -1)
        {
            // TODO: log light not found.
            return;
        }

        RemoveLight(lightIndex);
    }

    public void RemoveAllLights()
    {
        for (var i = lightControllers.Count - 1; i >= 0; i--)
            RemoveLight(i);
    }

    public IEnumerator<LightController> GetEnumerator() =>
        lightControllers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    void IActivateable.Activate() =>
        AddLight(GameMain.Instance.MainLight.GetComponent<Light>());

    void IActivateable.Deactivate() =>
        RemoveAllLights();

    internal static void DestroyParent()
    {
        if (!lightParent)
            return;

        Object.Destroy(lightParent);
    }

    private static bool IsMainLight(LightController lightController) =>
        lightController.Light == GameMain.Instance.MainLight.GetComponent<Light>();

    private static void ResetMainLight()
    {
        var light = GameMain.Instance.MainLight.GetComponent<Light>();

        light.enabled = true;
        light.type = LightType.Directional;
        light.transform.position = LightController.DefaultPosition;
    }

    private void BackupMainLight(LightController lightController)
    {
        if (!IsMainLight(lightController))
            return;

        initialMainLightProperties = LightProperties.FromLight(lightController.Light);
    }

    private void RestoreMainLight(LightController lightController)
    {
        if (!IsMainLight(lightController))
            return;

        lightController[LightType.Directional] = initialMainLightProperties;
    }
}
