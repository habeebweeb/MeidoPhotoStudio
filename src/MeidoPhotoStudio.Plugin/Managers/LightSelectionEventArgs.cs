using System;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightSelectionEventArgs : EventArgs
{
    public LightSelectionEventArgs(LightController lightController, int lightIndex)
    {
        LightController = lightController;
        LightIndex = lightIndex;
    }

    public LightController LightController { get; }

    public int LightIndex { get; }
}
