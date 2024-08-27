using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

/// <summary>General drag handle controller.</summary>
public abstract partial class GeneralDragHandleController : DragHandleControllerBase
{
    public const float DragHandleAlpha = 0.75f;

    protected static readonly Color MoveColour = new(0.2f, 0.5f, 0.95f, DragHandleAlpha);
    protected static readonly Color RotateColour = new(0.2f, 0.75f, 0.3f, DragHandleAlpha);
    protected static readonly Color ScaleColour = new(0.8f, 0.7f, 0.3f, DragHandleAlpha);
    protected static readonly Color SelectColour = new(0.9f, 0.5f, 1f, DragHandleAlpha);
    protected static readonly Color DeleteColour = new(1f, 0.1f, 0.1f, DragHandleAlpha);

    private DragHandleMode none;
    private DragHandleMode moveWorldXZ;
    private DragHandleMode moveWorldY;
    private DragHandleMode rotateLocalXZ;
    private DragHandleMode rotateWorldY;
    private DragHandleMode rotateLocalY;
    private DragHandleMode scale;
    private DragHandleMode select;
    private DragHandleMode delete;

    public GeneralDragHandleController(DragHandle dragHandle, Transform target)
        : base(dragHandle)
    {
        Target = target ? target : throw new ArgumentNullException(nameof(target));

        DragHandle.gameObject.SetActive(false);

        TransformBackup = new(Target);
    }

    public GeneralDragHandleController(DragHandle dragHandle, CustomGizmo gizmo, Transform target)
        : base(dragHandle, gizmo)
    {
        Target = target ? target : throw new ArgumentNullException(nameof(target));

        DragHandle.gameObject.SetActive(false);

        TransformBackup = new(Target);
    }

    public virtual DragHandleMode None =>
        none ??= new NoneMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode MoveWorldXZ =>
        moveWorldXZ ??= new MoveWorldXZMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode MoveWorldY =>
        moveWorldY ??= new MoveWorldYMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode RotateLocalXZ =>
        rotateLocalXZ ??= new RotateLocalXZMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode RotateWorldY =>
        rotateWorldY ??= new RotateWorldYMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode RotateLocalY =>
        rotateLocalY ??= new RotateLocalYMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode Scale =>
        scale ??= new ScaleMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode Select =>
        select ??= new SelectMode<GeneralDragHandleController>(this);

    public virtual DragHandleMode Delete =>
        delete ??= new DeleteMode<GeneralDragHandleController>(this);

    protected TransformBackup TransformBackup { get; set; }

    protected TransformBackup StartingTransform { get; set; }

    protected Transform Target { get; }
}
