using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointBody : DragPointGeneral
{
    public bool IsCube;

    private Meido meido;
    private bool isIK;

    public bool IsIK
    {
        get => isIK;
        set
        {
            if (isIK == value)
                return;

            isIK = value;
            ApplyDragType();
        }
    }

    public void Initialize(Meido meido, Func<Vector3> position, Func<Vector3> rotation)
    {
        Initialize(position, rotation);

        this.meido = meido;
    }

    public void Focus() =>
        meido.FocusOnBody();

    protected override void OnDoubleClick()
    {
        base.OnDoubleClick();

        if (Selecting)
            Focus();
    }

    protected override void ApplyDragType()
    {
        var enabled = !IsIK && (Transforming || Selecting);
        var select = IsIK && Selecting;

        ApplyProperties(enabled || select, IsCube && enabled, false);

        if (IsCube)
            ApplyColours();
    }
}
