namespace MeidoPhotoStudio.Plugin;

public class DragPointThigh : DragPointSpine
{
    protected override void ApplyDragType()
    {
        var current = CurrentDragType;

        if (!IsBone || current is DragHandleMode.Ignore)
        {
            ApplyProperties(false, false, false);

            return;
        }

        if (current is DragHandleMode.DragUpperBone)
            ApplyProperties(false, false, true);
        else
            ApplyProperties(false, false, false);
    }
}
