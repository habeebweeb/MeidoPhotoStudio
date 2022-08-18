namespace MeidoPhotoStudio.Plugin;

public class DragPointBody : DragPointGeneral
{
    public bool IsCube;

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

    protected override void ApplyDragType()
    {
        var enabled = !IsIK && (Transforming || Selecting);
        var select = IsIK && Selecting;

        ApplyProperties(enabled || select, IsCube && enabled, false);

        if (IsCube)
            ApplyColours();
    }
}
