namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class VirtualList(int bucketSize = 100) : IEnumerable<(int Index, Vector2 Offset)>
{
    private readonly Dictionary<int, float> offsetCache = [];
    private readonly List<float> offsetBuckets = [];

    private int bucketSize = bucketSize;
    private Vector2 scrollPosition;
    private Rect? scrollRect;
    private Rect scrollViewRect;
    private int columnCount;
    private int oldItemCount;
    private int firstVisibleIndex;
    private int lastVisibleIndex;
    private IVirtualListHandler handler = EmptyHandler.Instance;
    private bool grid;
    private Vector2 gridItemSize;
    private Vector2 spacing;

    public IVirtualListHandler Handler
    {
        get => handler;
        set
        {
            if (handler == value)
                return;

            handler = value ?? EmptyHandler.Instance;
            oldItemCount = handler.Count;

            Invalidate();
        }
    }

    public int BucketSize
    {
        get => bucketSize;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (bucketSize == value)
                return;

            bucketSize = value;

            Invalidate();
        }
    }

    public bool Grid
    {
        get => grid;
        set
        {
            if (grid == value)
                return;

            grid = value;

            Invalidate();
        }
    }

    public Vector2 Spacing
    {
        get => spacing;
        set
        {
            var newSpacing = value;

            if (newSpacing.x < 0f)
                newSpacing.x = 0f;

            if (newSpacing.y < 0f)
                newSpacing.y = 0f;

            if (spacing == newSpacing)
                return;

            spacing = newSpacing;

            Invalidate();
        }
    }

    public int ColumnCount =>
        Grid ? columnCount : 1;

    public Vector2 BeginScrollView(Rect scrollRect, Vector2 scrollPosition)
    {
        var repaint = Event.current.type is EventType.Repaint;

        if (repaint)
        {
            if (this.scrollRect != scrollRect || handler.Count != oldItemCount)
            {
                oldItemCount = handler.Count;

                this.scrollRect = scrollRect;
                this.scrollPosition = scrollPosition;

                InitializeScrollView(scrollRect);
                RecalculateVisibleIndices(scrollRect, scrollPosition);
            }
        }

        var newScrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, scrollViewRect);

        if (repaint)
        {
            if (newScrollPosition != this.scrollPosition)
            {
                this.scrollPosition = newScrollPosition;

                RecalculateVisibleIndices(scrollRect, newScrollPosition);
            }
        }

        return newScrollPosition;

        void InitializeScrollView(Rect scrollRect)
        {
            offsetBuckets.Clear();

            var scrollViewHeight = 0f;

            if (Grid)
            {
                var itemDimensions = handler.ItemDimensions(0);

                columnCount = (int)((scrollRect.width + Spacing.x) / (itemDimensions.x + Spacing.x));

                scrollViewHeight = (itemDimensions.y + Spacing.x) * Mathf.CeilToInt((float)handler.Count / columnCount) - Spacing.x;

                gridItemSize = itemDimensions + Spacing;
            }
            else
            {
                for (var i = 0; i < handler.Count; i++)
                {
                    if (i % BucketSize is 0)
                        offsetBuckets.Add(scrollViewHeight);

                    var itemDimensions = handler.ItemDimensions(i);

                    scrollViewHeight += itemDimensions.y + Spacing.y;
                }

                scrollViewHeight -= Spacing.y;
            }

            scrollViewRect = new(scrollRect.x, scrollRect.y, scrollRect.width - 18f, scrollViewHeight);
        }

        void RecalculateVisibleIndices(Rect scrollRect, Vector2 newScrollPosition)
        {
            if (offsetBuckets.Count is 0 && !Grid)
            {
                firstVisibleIndex = 0;
                lastVisibleIndex = 0;
                offsetCache.Clear();

                return;
            }

            var (newFirstVisibleIndex, newLastVisibleIndex) = CalculateVisibleIndices(newScrollPosition.y, scrollRect.height);

            if (newFirstVisibleIndex != firstVisibleIndex || newLastVisibleIndex != lastVisibleIndex)
                offsetCache.Clear();

            firstVisibleIndex = newFirstVisibleIndex;
            lastVisibleIndex = newLastVisibleIndex;

            (int, int) CalculateVisibleIndices(float scrollPosition, float scrollRectHeight)
            {
                int firstVisibleIndex;
                int lastVisibleIndex;

                if (Grid)
                {
                    firstVisibleIndex = Mathf.FloorToInt((scrollPosition + Spacing.y) / (handler.ItemDimensions(0).y + Spacing.y)) * columnCount;
                    lastVisibleIndex = Mathf.CeilToInt((scrollPosition + scrollRectHeight + Spacing.y) / (handler.ItemDimensions(0).y + Spacing.y)) * columnCount + columnCount;

                    if (firstVisibleIndex < 0)
                        firstVisibleIndex = 0;

                    if (lastVisibleIndex > handler.Count)
                        lastVisibleIndex = handler.Count;
                }
                else
                {
                    (firstVisibleIndex, var offset) = CalculateFirstVisibleIndex(scrollPosition);
                    lastVisibleIndex = CalculateLastVisibleIndex(firstVisibleIndex, offset, scrollPosition, scrollRectHeight);
                }

                return (firstVisibleIndex, lastVisibleIndex);

                (int, float) CalculateFirstVisibleIndex(float scrollPosition)
                {
                    if (scrollPosition > offsetBuckets[offsetBuckets.Count - 1])
                        return ((offsetBuckets.Count - 1) * BucketSize, offsetBuckets[offsetBuckets.Count - 1]);

                    var bucketIndex = offsetBuckets.BinarySearch(scrollPosition);

                    if (bucketIndex < 0)
                        bucketIndex = ~bucketIndex - 1;

                    var offset = offsetBuckets[bucketIndex];

                    for (var i = bucketIndex * BucketSize; i < handler.Count; i++)
                    {
                        offset += handler.ItemDimensions(i).y + Spacing.y;

                        if (offset >= scrollPosition)
                            return (i, offset);
                    }

                    return (bucketIndex * BucketSize, offsetBuckets[bucketIndex]);
                }

                int CalculateLastVisibleIndex(int firstVisibleIndex, float firstVisibleOffset, float scrollPosition, float scrollRectHeight)
                {
                    var endPosition = scrollPosition + scrollRectHeight;

                    if (endPosition > offsetBuckets[offsetBuckets.Count - 1])
                        return handler.Count;

                    var bucketIndex = offsetBuckets.BinarySearch(endPosition);

                    if (bucketIndex < 0)
                        bucketIndex = ~bucketIndex;

                    var offset = firstVisibleOffset;

                    for (var i = firstVisibleIndex + 1; i < handler.Count; i++)
                    {
                        if (offset >= endPosition)
                            return i;

                        offset += handler.ItemDimensions(i).y + Spacing.y;
                    }

                    return bucketIndex * BucketSize;
                }
            }
        }
    }

    public IEnumerator<(int Index, Vector2 Offset)> GetEnumerator()
    {
        if (Grid)
        {
            for (var i = firstVisibleIndex; i < lastVisibleIndex; i += columnCount)
            {
                for (var j = 0; j < columnCount; j++)
                {
                    if (i + j >= handler.Count)
                        yield break;

                    yield return (i + j, new(gridItemSize.x * j, gridItemSize.y * (i / columnCount)));
                }
            }
        }
        else
        {
            var lastVisibleIndex = Mathf.Min(this.lastVisibleIndex, handler.Count);

            for (var i = firstVisibleIndex; i < lastVisibleIndex; i++)
                yield return (i, GetItemOffset(i));
        }

        Vector2 GetItemOffset(int index)
        {
            if (offsetCache.TryGetValue(index, out var offset))
                return new(0f, offset);

            var bucketIndex = index / BucketSize;
            var startingHeight = offsetBuckets[bucketIndex];

            var offsetWithinBucket = 0f;
            var bucketStartIndex = bucketIndex * BucketSize;

            for (var i = bucketStartIndex; i < index; i++)
            {
                offsetWithinBucket += handler.ItemDimensions(i).y;

                if (index != 0)
                    offsetWithinBucket += Spacing.y;
            }

            offset = startingHeight + offsetWithinBucket;

            offsetCache[index] = offset;

            return new(0f, offset);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Invalidate()
    {
        scrollRect = null;
        scrollPosition = Vector2.zero;
        scrollViewRect = Rect.zero;
        offsetBuckets.Clear();
        firstVisibleIndex = 0;
        lastVisibleIndex = 0;
    }

    private sealed class EmptyHandler : IVirtualListHandler
    {
        private static EmptyHandler instance;

        public static EmptyHandler Instance =>
            instance ??= new();

        public int Count { get; }

        public Vector2 ItemDimensions(int index) =>
            Vector2.zero;
    }
}
