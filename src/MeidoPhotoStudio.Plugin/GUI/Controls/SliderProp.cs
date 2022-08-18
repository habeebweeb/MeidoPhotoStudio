namespace MeidoPhotoStudio.Plugin;

public readonly struct SliderProp
{
    public SliderProp(float left, float right, float initial = 0f, float @default = 0f)
    {
        Left = left;
        Right = right;
        Initial = Utility.Bound(initial, left, right);
        Default = Utility.Bound(@default, left, right);
    }

    public float Left { get; }

    public float Right { get; }

    public float Initial { get; }

    public float Default { get; }
}
