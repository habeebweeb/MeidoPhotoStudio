namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightRepositoryEventArgs(LightController lightController, int lightIndex) : EventArgs
{
    public LightController LightController { get; } = lightController
        ?? throw new ArgumentNullException(nameof(lightController));

    public int LightIndex { get; } = lightIndex;
}
