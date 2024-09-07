namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightSelectionEventArgs(LightController lightController, int lightIndex) : EventArgs
{
    public LightController LightController { get; } = lightController;

    public int LightIndex { get; } = lightIndex;
}
