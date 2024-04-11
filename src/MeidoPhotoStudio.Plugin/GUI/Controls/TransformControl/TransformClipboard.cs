namespace MeidoPhotoStudio.Plugin;

public class TransformClipboard
{
    private readonly Dictionary<TransformType, Vector3?> transformInformation = [];

    public enum TransformType
    {
        Position,
        Rotation,
        Scale,
    }

    public Vector3? this[TransformType type]
    {
        get => transformInformation.TryGetValue(type, out var value) ? value : null;
        set => transformInformation[type] = value;
    }

    public void Clear() =>
        transformInformation.Clear();
}
