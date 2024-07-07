namespace MeidoPhotoStudio.Plugin.Core;

public class TransformChangeEventArgs(TransformChangeEventArgs.TransformType transformType) : EventArgs
{
    [Flags]
    public enum TransformType
    {
        None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    public TransformType Type { get; } = transformType;
}
