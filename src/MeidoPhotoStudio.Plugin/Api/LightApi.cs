using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Lighting;

namespace MeidoPhotoStudio.Plugin.Api;

public class LightApi : ApiBase
{
    private readonly LightService lightService;
    private readonly SelectionController<LightController> lightSelectionController;

    public LightApi(
        PluginCore pluginCore,
        LightService lightService,
        SelectionController<LightController> lightSelectionController)
        : base(pluginCore)
    {
        this.lightService = lightService ?? throw new ArgumentNullException(nameof(lightService));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));

        this.lightService.AddedLight += OnLightAdded;
        this.lightService.RemovingLight += OnLightRemoving;
        this.lightService.RemovedLight += OnLightRemoved;
    }

    public event EventHandler<LightServiceEventArgs> AddedLight;

    public event EventHandler<LightServiceEventArgs> RemovingLight;

    public event EventHandler<LightServiceEventArgs> RemovedLight;

    public IEnumerable<LightController> Lights
    {
        get
        {
            Valid();

            return lightService;
        }
    }

    public LightController SelectedLight
    {
        get
        {
            Valid();

            return lightSelectionController.Current;
        }

        set
        {
            Valid();

            _ = value ?? throw new ArgumentNullException(nameof(value));

            lightSelectionController.Select(value);
        }
    }

    public int SelectedLightIndex
    {
        get
        {
            Valid();

            return lightSelectionController.CurrentIndex;
        }

        set
        {
            Valid();

            if ((uint)value >= lightService.Count)
                throw new ArgumentOutOfRangeException(nameof(value));

            lightSelectionController.Select(value);
        }
    }

    public int LightCount
    {
        get
        {
            Valid();

            return lightService.Count;
        }
    }

    public LightController this[int index]
    {
        get
        {
            Valid();

            return (uint)index >= lightService.Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : lightService[index];
        }
    }

    public LightController AddLight()
    {
        Valid();

        LightController light = null;

        try
        {
            lightService.AddedLight += OnLightAdded;
            lightService.AddLight();
        }
        catch
        {
            throw;
        }
        finally
        {
            lightService.AddedLight -= OnLightAdded;
        }

        return light;

        void OnLightAdded(object sender, LightServiceEventArgs e) =>
            light = e.LightController;
    }

    public void RemoveLight(int index)
    {
        Valid();

        if ((uint)index >= lightService.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        lightService.RemoveLight(index);
    }

    public void RemoveLight(LightController light)
    {
        Valid();

        _ = light ?? throw new ArgumentNullException(nameof(light));

        lightService.RemoveLight(light);
    }

    public void RemoveAllLights()
    {
        Valid();

        lightService.RemoveAllLights();
    }

    private void OnLightAdded(object sender, LightServiceEventArgs e) =>
        AddedLight?.Invoke(this, e);

    private void OnLightRemoving(object sender, LightServiceEventArgs e) =>
        RemovingLight?.Invoke(this, e);

    private void OnLightRemoved(object sender, LightServiceEventArgs e) =>
        RemovedLight?.Invoke(this, e);
}
