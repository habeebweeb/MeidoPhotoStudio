namespace MeidoPhotoStudio.Plugin.Core.Character;

public class TransformChangeEventArgs(TransformChangeEventArgs.TransformType transformType) : EventArgs
{
    public enum TransformType
    {
        Position,
        Rotation,
        Scale,
    }

    public TransformType Type { get; } = transformType;
}
