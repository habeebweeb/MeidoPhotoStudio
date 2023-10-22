using System;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightSelectionController
{
    private readonly LightRepository lightRepository;

    public LightSelectionController(LightRepository lightRepository) =>
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));

    public event EventHandler<LightSelectionEventArgs> Selected;

    public void Select(int index)
    {
        if ((uint)index >= lightRepository.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Selected?.Invoke(this, new(lightRepository[index], index));
    }

    public void Select(LightController lightController)
    {
        if (lightController is null)
            throw new ArgumentNullException(nameof(lightController));

        var lightIndex = lightRepository.IndexOf(lightController);

        if (lightIndex is -1)
            return;

        Selected?.Invoke(this, new(lightController, lightIndex));
    }
}
