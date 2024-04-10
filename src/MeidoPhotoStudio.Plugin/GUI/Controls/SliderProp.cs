namespace MeidoPhotoStudio.Plugin;

public readonly record struct SliderProp(float Left, float Right, float Initial = 0f, float Default = 0f)
{
    public float Initial { get; } = Utility.Bound(Initial, Left, Right);

    public float Default { get; } = Utility.Bound(Default, Left, Right);
}
