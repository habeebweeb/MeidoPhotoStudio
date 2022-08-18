namespace MeidoPhotoStudio.Plugin;

// TODO: Extract other classes to another file
public class DragPointPropDTO
{
    public DragPointPropDTO()
    {
    }

    public DragPointPropDTO(DragPointProp dragPoint)
    {
        TransformDTO = new(dragPoint.MyObject.transform);
        ShadowCasting = dragPoint.ShadowCasting;
        AttachPointInfo = dragPoint.AttachPointInfo;
        PropInfo = dragPoint.Info;
    }

    public TransformDTO TransformDTO { get; set; }

    public AttachPointInfo AttachPointInfo { get; set; }

    public PropInfo PropInfo { get; set; }

    public bool ShadowCasting { get; set; }

    public void Deconstruct(out TransformDTO transform, out AttachPointInfo attachPointInfo, out bool shadowCasting)
    {
        transform = TransformDTO;
        attachPointInfo = AttachPointInfo;
        shadowCasting = ShadowCasting;
    }
}
