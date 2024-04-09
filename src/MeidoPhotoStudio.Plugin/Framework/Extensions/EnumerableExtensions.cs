using System;
using System.Collections.Generic;

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));

        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext())
            yield return ChunkIterator(enumerator, chunkSize - 1);

        static IEnumerable<T> ChunkIterator(IEnumerator<T> enumerator, int chunkSize)
        {
            yield return enumerator.Current;

            for (var i = 0; i < chunkSize && enumerator.MoveNext(); i++)
                yield return enumerator.Current;
        }
    }

    public static IEnumerable<(TFirst First, TSecond Second)>
        Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
    {
        if (first == null)
            throw new ArgumentNullException(nameof(first));

        if (second == null)
            throw new ArgumentNullException(nameof(second));

        using var firstEnumerator = first.GetEnumerator();
        using var secondEnumerator = second.GetEnumerator();

        while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
            yield return (firstEnumerator.Current, secondEnumerator.Current);
    }

    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();

        var index = 0;

        while (enumerator.MoveNext())
            yield return (index++, enumerator.Current);
    }

    public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var index = 0;

        foreach (var item in source)
        {
            if (predicate(item))
                return index;

            index++;
        }

        return -1;
    }
}
