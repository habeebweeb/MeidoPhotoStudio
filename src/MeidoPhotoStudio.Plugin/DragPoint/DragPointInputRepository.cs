using System;
using System.Collections.Generic;

using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public abstract class DragPointInputRepository<T> : IDragPointInputRepository<T>
    where T : DragPoint
{
    protected readonly InputConfiguration inputConfiguration;

    private readonly List<T> dragPoints = new();

    private DragHandleMode currentDragType;

    public DragPointInputRepository(InputConfiguration inputConfiguration) =>
        this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));

    public virtual bool Active =>
        dragPoints.Count > 0;

    private DragHandleMode CurrentDragType
    {
        get => currentDragType;
        set
        {
            if (value == currentDragType)
                return;

            currentDragType = value;

            NotifyDragTypeChange();
        }
    }

    public void AddDragHandle(T dragPoint)
    {
        if (dragPoint == null)
            throw new ArgumentNullException(nameof(dragPoint));

        if (dragPoints.Contains(dragPoint))
            return;

        dragPoints.Add(dragPoint);

        dragPoint.CurrentDragType = CurrentDragType;
    }

    public void RemoveDragHandle(T dragPoint)
    {
        if (dragPoint == null)
            throw new ArgumentNullException(nameof(dragPoint));

        if (!dragPoints.Contains(dragPoint))
            return;

        dragPoints.Remove(dragPoint);
    }

    public virtual void CheckInput() =>
        CurrentDragType = CheckDragType();

    protected abstract DragHandleMode CheckDragType();

    private void NotifyDragTypeChange()
    {
        foreach (var dragPoint in dragPoints)
            dragPoint.CurrentDragType = CurrentDragType;
    }
}
