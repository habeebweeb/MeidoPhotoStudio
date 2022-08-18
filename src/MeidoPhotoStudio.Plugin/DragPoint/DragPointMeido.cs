using System;

using UnityEngine;

using static MeidoPhotoStudio.Plugin.CustomGizmo;

namespace MeidoPhotoStudio.Plugin;

public abstract class DragPointMeido : DragPoint
{
    public static readonly Vector3 BoneScale = Vector3.one * 0.04f;

    protected const int JointUpper = 0;
    protected const int JointMiddle = 1;
    protected const int JointLower = 2;

    protected Meido meido;
    protected Maid maid;
    protected bool isPlaying;
    protected bool isBone;

    public virtual bool IsBone
    {
        get => isBone;
        set
        {
            if (value == isBone)
                return;

            isBone = value;
            ApplyDragType();
        }
    }

    protected IKCtrlData IkCtrlData =>
        meido.Body.IKCtrl.GetIKData("左手");

    public virtual void Initialize(Meido meido, Func<Vector3> position, Func<Vector3> rotation)
    {
        Initialize(position, rotation);

        this.meido = meido;
        maid = meido.Maid;
        isPlaying = !meido.Stop;
    }

    public override void AddGizmo(float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
    {
        base.AddGizmo(scale, mode);

        Gizmo.GizmoDrag += (_, _) =>
        {
            meido.Stop = true;
            isPlaying = false;
        };
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        isPlaying = !meido.Stop;
    }

    protected void InitializeIK(TBody.IKCMO iKCmo, Transform upper, Transform middle, Transform lower) =>
        iKCmo.Init(upper, middle, lower, maid.body0);

    protected void Porc(TBody.IKCMO ikCmo, IKCtrlData ikData, Transform upper, Transform middle, Transform lower) =>
        ikCmo.Porc(upper, middle, lower, CursorPosition(), Vector3.zero, ikData);
}
