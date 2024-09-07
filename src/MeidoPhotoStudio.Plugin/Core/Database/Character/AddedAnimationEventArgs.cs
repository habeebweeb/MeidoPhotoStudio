namespace MeidoPhotoStudio.Plugin.Core.Database.Character;

public class AddedAnimationEventArgs(IAnimationModel animation) : EventArgs
{
    public IAnimationModel Animation { get; } = animation ?? throw new ArgumentNullException(nameof(animation));
}