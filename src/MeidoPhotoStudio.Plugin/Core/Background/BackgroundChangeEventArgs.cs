using MeidoPhotoStudio.Plugin.Core.Database.Background;

namespace MeidoPhotoStudio.Plugin.Core.Background;

public class BackgroundChangeEventArgs(BackgroundModel backgroundInfo, Transform backgroundTransform) : EventArgs
{
    public Transform BackgroundTransform { get; } = backgroundTransform;

    public BackgroundModel BackgroundInfo { get; } = backgroundInfo;
}
