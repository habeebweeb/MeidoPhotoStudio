using System;
using System.Collections;
using System.Collections.Generic;

namespace MeidoPhotoStudio.Plugin.Framework.Collections;

public class SelectList<T> : ISelectList<T>
{
    private readonly IList<T> items;

    private int currentIndex;

    public SelectList() =>
        items = new List<T>();

    public SelectList(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        items = new List<T>(collection);
    }

    public SelectList(IEnumerable<T> collection, int currentIndex)
        : this(collection) =>
            this.currentIndex = (uint)currentIndex >= items.Count
                ? throw new ArgumentOutOfRangeException(nameof(currentIndex))
                : currentIndex;

    private enum StepDirection
    {
        Left,
        Right,
    }

    public bool IsReadOnly =>
        false;

    public int Count =>
        items.Count;

    public T Current
    {
        get
        {
            if (items.Count is 0)
                throw new InvalidOperationException("Items is empty");

            return items[CurrentIndex];
        }

        set
        {
            if (items.Count is 0)
                throw new InvalidOperationException("Items is empty");

            items[CurrentIndex] = value;
        }
    }

    public int CurrentIndex
    {
        get => currentIndex;
        set
        {
            if (items.Count is 0)
                throw new InvalidOperationException("Items is empty");

            if ((uint)value >= items.Count)
                throw new IndexOutOfRangeException(nameof(value));

            currentIndex = value;
        }
    }

    public T this[int index]
    {
        get => items[index];
        set
        {
            if ((uint)index >= items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            items[index] = value;
        }
    }

    public T SetCurrentIndex(int index)
    {
        if (items.Count is 0)
            throw new InvalidOperationException("Items is empty");

        if ((uint)index >= items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        currentIndex = index;

        return Current;
    }

    public T Next() =>
        Step(StepDirection.Right);

    public T Previous() =>
        Step(StepDirection.Left);

    public T CycleNext() =>
        Step(StepDirection.Right, true);

    public T CyclePrevious() =>
        Step(StepDirection.Left, true);

    public IEnumerator<T> GetEnumerator() =>
        items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(T item) =>
        items.Add(item);

    public void Clear()
    {
        items.Clear();

        currentIndex = 0;
    }

    public bool Contains(T item) =>
       items.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) =>
        items.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        var result = items.Remove(item);

        if (result && currentIndex >= items.Count)
            currentIndex = items.Count - 1;

        return result;
    }

    public int IndexOf(T item) =>
        items.IndexOf(item);

    public void Insert(int index, T item) =>
        items.Insert(index, item);

    public void RemoveAt(int index)
    {
        items.RemoveAt(index);

        if (currentIndex >= items.Count)
            currentIndex = items.Count - 1;
    }

    private T Step(StepDirection direction, bool cycle = false)
    {
        if (items.Count is 0)
            throw new InvalidOperationException("Items is empty");

        CurrentIndex += direction is StepDirection.Left ? -1 : 1;

        if (CurrentIndex >= items.Count)
            CurrentIndex = cycle ? 0 : items.Count - 1;
        else if (CurrentIndex < 0)
            CurrentIndex = cycle ? items.Count - 1 : 0;

        return items[CurrentIndex];
    }
}
