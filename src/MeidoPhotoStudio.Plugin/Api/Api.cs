using MeidoPhotoStudio.Plugin.Api.Extension;
using MeidoPhotoStudio.Plugin.Core;

namespace MeidoPhotoStudio.Plugin.Api;

public class Api(
    PluginCore pluginCore,
    CharacterApi characterApi,
    PropApi propApi,
    LightApi lightApi,
    UIApi uiApi,
    ExtensionApi extensionApi)
{
    private readonly PluginCore pluginCore = pluginCore ? pluginCore : throw new ArgumentNullException(nameof(pluginCore));

    public event EventHandler Activating;

    public event EventHandler Activated;

    public event EventHandler Deactivating;

    public event EventHandler Deactivated;

    public bool Active =>
        pluginCore.Active;

    public CharacterApi Character { get; } = characterApi ?? throw new ArgumentNullException(nameof(characterApi));

    public PropApi Prop { get; } = propApi ?? throw new ArgumentNullException(nameof(propApi));

    public LightApi Light { get; } = lightApi ?? throw new ArgumentNullException(nameof(lightApi));

    public UIApi UI { get; } = uiApi ?? throw new ArgumentNullException(nameof(uiApi));

    public ExtensionApi Extension { get; } = extensionApi ?? throw new ArgumentNullException(nameof(extensionApi));

    internal void RaiseActivating() =>
        Activating?.Invoke(this, EventArgs.Empty);

    internal void RaiseActivated() =>
        Activated?.Invoke(this, EventArgs.Empty);

    internal void RaiseDeactivating() =>
        Deactivating?.Invoke(this, EventArgs.Empty);

    internal void RaiseDeactivated() =>
        Deactivated?.Invoke(this, EventArgs.Empty);
}
