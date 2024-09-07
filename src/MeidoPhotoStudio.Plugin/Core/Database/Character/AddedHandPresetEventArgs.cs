namespace MeidoPhotoStudio.Plugin.Core.Database.Character;

public class AddedHandPresetEventArgs(HandPresetModel handPreset) : EventArgs
{
    public HandPresetModel HandPreset { get; } = handPreset ?? throw new ArgumentNullException(nameof(handPreset));
}
