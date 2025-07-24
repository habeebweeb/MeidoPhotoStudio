using System.Collections;

namespace COM3D2X.SafeEnum;

internal readonly struct ValueCollection<T>(IEnumerable<T> collection) : IEnumerable<T>, IEquatable<ValueCollection<T>>
    where T : IEquatable<T>
{
    private readonly T[] collection = [.. collection ?? throw new ArgumentNullException()];

    public ReadOnlySpan<T> AsSpan() =>
        collection.AsSpan();

    public bool Equals(ValueCollection<T> other) =>
        AsSpan().SequenceEqual(other.AsSpan());

    public override bool Equals(object other) =>
        other is ValueCollection<T> valueCollection && Equals(valueCollection);

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);

        foreach (var item in collection)
            hashCode.Add(item);

        return hashCode.ToHashCode();
    }

    public IEnumerator<T> GetEnumerator() =>
        ((IEnumerable<T>)collection).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
