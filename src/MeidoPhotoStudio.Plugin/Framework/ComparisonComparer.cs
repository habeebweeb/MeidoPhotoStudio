namespace MeidoPhotoStudio.Plugin.Framework;

public static class ComparisonComparer<T>
{
    public static IComparer<T> Create(Comparison<T> comparison) =>
        comparison is null
            ? throw new ArgumentNullException(nameof(comparison))
            : new Comparer(comparison);

    private sealed class Comparer(Comparison<T> comparison) : Comparer<T>
    {
        public override int Compare(T a, T b) =>
            comparison(a, b);
    }
}
