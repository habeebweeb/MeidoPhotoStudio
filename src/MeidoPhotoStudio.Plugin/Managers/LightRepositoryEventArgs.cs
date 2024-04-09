namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightRepositoryEventArgs : EventArgs
{
    public LightRepositoryEventArgs(LightController lightController, int lightIndex)
    {
        LightController = lightController ?? throw new ArgumentNullException(nameof(lightController));
        LightIndex = lightIndex;
    }

    public LightController LightController { get; }

    public int LightIndex { get; }
}
