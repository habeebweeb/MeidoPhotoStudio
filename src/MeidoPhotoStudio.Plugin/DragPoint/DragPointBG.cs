using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointBG : DragPointGeneral
{
    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        DefaultPosition = myObject.position;
    }

    protected override void ApplyDragType()
    {
        ApplyProperties(Transforming, Transforming, Rotating);
        ApplyColours();
    }
}
