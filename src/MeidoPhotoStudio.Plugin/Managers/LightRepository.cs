namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightRepository(TransformWatcher transformWatcher) : IEnumerable<LightController>, IIndexableCollection<LightController>
{
    private static GameObject lightParent;

    private readonly List<LightController> lightControllers = [];
    private readonly TransformWatcher transformWatcher =
        transformWatcher ? transformWatcher : throw new ArgumentNullException(nameof(transformWatcher));

    public event EventHandler<LightRepositoryEventArgs> AddedLight;

    public event EventHandler<LightRepositoryEventArgs> RemovingLight;

    public event EventHandler<LightRepositoryEventArgs> RemovedLight;

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
            ResetMainLight();

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
            ResetMainLight();

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

    private static bool IsMainLight(LightController lightController) =>
        lightController.Light == GameMain.Instance.MainLight.GetComponent<Light>();

    private static void ResetMainLight()
    {
        var light = GameMain.Instance.MainLight.GetComponent<Light>();

        light.type = LightType.Directional;
        light.transform.position = LightController.DefaultPosition;

        GameMain.Instance.MainLight.Reset();
    }
}
