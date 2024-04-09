namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class FaceShapeKeyConfigurationEventArgs(string changedShapeKeys) : EventArgs
{
    public string ChangedShapeKey { get; } = changedShapeKeys ?? throw new ArgumentNullException(nameof(changedShapeKeys));
}
