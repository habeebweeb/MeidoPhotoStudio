namespace MeidoPhotoStudio.Plugin;

public class DragPointHip : DragPointSpine
{
    protected override void ApplyDragType()
    {
        var current = CurrentDragType;

        if (!IsBone || current is DragHandleMode.Ignore)
        {
            ApplyProperties(false, false, false);

            return;
        }

        if (current is DragHandleMode.None or DragHandleMode.MoveLocalY)
            ApplyProperties(true, true, false);
        else if (current is DragHandleMode.HipBoneRotation)
            ApplyProperties(false, false, true);
        else
            ApplyProperties(false, false, false);
    }

    protected override void Drag()
    {
        base.Drag();

        if (CurrentDragType is DragHandleMode.MoveLocalY)
        {
            var cursorPosition = CursorPosition();

            MyObject.position = new(MyObject.position.x, cursorPosition.y, MyObject.position.z);
        }
    }
}
