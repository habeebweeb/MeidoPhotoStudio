namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class ShapeKeyRangeConfiguration(IShapeKeyRangeSerializer shapeKeyRangeSerializer)
{
    private readonly IShapeKeyRangeSerializer shapeKeyRangeSerializer = shapeKeyRangeSerializer
        ?? throw new ArgumentException(nameof(shapeKeyRangeSerializer));

    private Dictionary<string, ShapeKeyRange> ranges;

    public event EventHandler Refreshed;

    private Dictionary<string, ShapeKeyRange> Ranges =>
        ranges ??= Initialize(shapeKeyRangeSerializer);

    public ShapeKeyRange this[string shapeKey] =>
        string.IsNullOrEmpty(shapeKey)
            ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
            : Ranges[shapeKey];

    public bool TryGetRange(string shapeKey, out ShapeKeyRange range) =>
        string.IsNullOrEmpty(shapeKey)
            ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
            : Ranges.TryGetValue(shapeKey, out range);

    public bool ContainsKey(string shapeKey) =>
        string.IsNullOrEmpty(shapeKey)
            ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
            : Ranges.ContainsKey(shapeKey);

    public void Refresh()
    {
        ranges = Initialize(shapeKeyRangeSerializer);

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    private static Dictionary<string, ShapeKeyRange> Initialize(IShapeKeyRangeSerializer shapeKeyRangeSerializer)
    {
        return shapeKeyRangeSerializer
            .Deserialize()
            .ToDictionary(kvp => kvp.Key, kvp => FixRange(kvp.Value), StringComparer.OrdinalIgnoreCase);

        static ShapeKeyRange FixRange(ShapeKeyRange range) =>
            range.Lower == range.Upper ? new(0f, 1f) :
            range.Lower > range.Upper ? new(range.Upper, range.Lower) :
            range;
    }
}
