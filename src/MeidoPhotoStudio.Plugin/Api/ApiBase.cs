using MeidoPhotoStudio.Plugin.Core;

namespace MeidoPhotoStudio.Plugin.Api;

public abstract class ApiBase(PluginCore pluginCore)
{
    private readonly PluginCore pluginCore = pluginCore
        ? pluginCore : throw new ArgumentNullException(nameof(pluginCore));

    protected virtual void Valid()
    {
        if (!pluginCore.Active)
            throw new InvalidOperationException($"{Plugin.PluginName} is not active");
    }
}
