namespace MeidoPhotoStudio.Plugin;

public class DragPointHip : DragPointSpine
{
    protected override void ApplyDragType()
    {
        var current = CurrentDragType;

        if (!IsBone || current is LegacyDragHandleMode.Ignore)
        {
            ApplyProperties(false, false, false);

            return;
        }

        if (current is LegacyDragHandleMode.None or LegacyDragHandleMode.MoveLocalY)
            ApplyProperties(true, true, false);
        else if (current is LegacyDragHandleMode.HipBoneRotation)
            ApplyProperties(false, false, true);
        else
            ApplyProperties(false, false, false);
    }

    protected override void Drag()
    {
        base.Drag();

        if (CurrentDragType is LegacyDragHandleMode.MoveLocalY)
        {
            var cursorPosition = CursorPosition();

            MyObject.position = new(MyObject.position.x, cursorPosition.y, MyObject.position.z);
        }
    }
}
