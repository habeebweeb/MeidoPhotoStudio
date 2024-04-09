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

    private NoneMode none;
    private MoveWorldXZMode moveWorldXZ;
    private MoveWorldYMode moveWorldY;
    private RotateLocalXZMode rotateLocalXZ;
    private RotateWorldYMode rotateWorldY;
    private RotateLocalYMode rotateLocalY;
    private ScaleMode scale;
    private SelectMode select;
    private DeleteMode delete;

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

    public virtual GeneralDragHandleMode<GeneralDragHandleController> None =>
        none ??= new NoneMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> MoveWorldXZ =>
        moveWorldXZ ??= new MoveWorldXZMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> MoveWorldY =>
        moveWorldY ??= new MoveWorldYMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> RotateLocalXZ =>
        rotateLocalXZ ??= new RotateLocalXZMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> RotateWorldY =>
        rotateWorldY ??= new RotateWorldYMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> RotateLocalY =>
        rotateLocalY ??= new RotateLocalYMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> Scale =>
        scale ??= new ScaleMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> Select =>
        select ??= new SelectMode(this);

    public virtual GeneralDragHandleMode<GeneralDragHandleController> Delete =>
        delete ??= new DeleteMode(this);

    protected TransformBackup TransformBackup { get; set; }

    protected Transform Target { get; }
}
