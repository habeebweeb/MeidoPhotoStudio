using System;
using System.Collections.Generic;

using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public abstract class DragPointInputRepository<T> : IDragPointInputRepository<T>
    where T : DragPoint
{
    private readonly List<T> dragPoints = new();

    private DragType currentDragType;

    public virtual bool Active =>
        dragPoints.Count > 0;

    private DragType CurrentDragType
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

    protected static bool OtherDragType() =>
        InputManager.GetKey(MpsKey.DragSelect) || InputManager.GetKey(MpsKey.DragDelete)
        || InputManager.GetKey(MpsKey.DragMove) || InputManager.GetKey(MpsKey.DragRotate)
        || InputManager.GetKey(MpsKey.DragScale) || InputManager.GetKey(MpsKey.DragFinger);

    protected abstract DragType CheckDragType();

    private void NotifyDragTypeChange()
    {
        foreach (var dragPoint in dragPoints)
            dragPoint.CurrentDragType = CurrentDragType;
    }
}
