namespace MeidoPhotoStudio.Plugin;

public readonly struct SliderProp(float left, float right, float initial = 0f, float @default = 0f)
{
    public float Left { get; } = left;

    public float Right { get; } = right;

    public float Initial { get; } = Utility.Bound(initial, left, right);

    public float Default { get; } = Utility.Bound(@default, left, right);
}
