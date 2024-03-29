using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin;

public class DragPointProp : DragPointGeneral
{
    public string AssetName = string.Empty;

    private List<Renderer> renderers;

    public AttachPointInfo AttachPointInfo { get; private set; } = AttachPointInfo.Empty;

    public PropInfo Info { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool ShadowCasting
    {
        get => renderers.Count is not 0 && renderers.Any(r => r.shadowCastingMode is ShadowCastingMode.On);
        set
        {
            foreach (var renderer in renderers)
                renderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }
    }

    public bool Visible
    {
        get => renderers.Count is not 0 && renderers.Any(renderer => renderer.enabled);
        set
        {
            foreach (var renderer in renderers)
                renderer.enabled = value;
        }
    }

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        DefaultRotation = MyObject.rotation;
        DefaultPosition = MyObject.position;
        DefaultScale = MyObject.localScale;
        renderers = new(MyObject.GetComponentsInChildren<Renderer>());
    }

    public void AttachTo(Meido meido, AttachPoint point, bool keepWorldPosition = true)
    {
        var attachPoint = meido?.IKManager.GetAttachPointTransform(point);

        AttachPointInfo = meido is null ? AttachPointInfo.Empty : new(point, meido);

        var rotation = MyObject.rotation;
        var scale = MyObject.localScale;

        MyObject.transform.SetParent(attachPoint, keepWorldPosition);

        if (keepWorldPosition)
        {
            MyObject.rotation = rotation;
        }
        else
        {
            MyObject.localPosition = Vector3.zero;
            MyObject.rotation = Quaternion.identity;
        }

        MyObject.localScale = scale;
    }

    public void DetachFrom(bool keepWorldPosition = true) =>
        AttachTo(null, AttachPoint.None, keepWorldPosition);

    public void DetachTemporary() =>
        MyObject.transform.SetParent(null, true);

    public void Focus()
    {
        var propPosition = MyObject.position;
        var cameraAngle = camera.transform.eulerAngles;
        var cameraDistance = GameMain.Instance.MainCamera.GetDistance();

        WfCameraMoveSupportUtility.StartMove(propPosition, cameraDistance, new(cameraAngle.y, cameraAngle.x), 0.45f);
    }

    protected override void ApplyDragType()
    {
        var widgetActiveContext = Transforming || Scaling || Rotating;
        var dragPointActive = DragPointEnabled && (widgetActiveContext || Special);
        var gizmoActive = GizmoEnabled && widgetActiveContext;

        ApplyProperties(dragPointActive, dragPointActive, gizmoActive);
        ApplyColours();

        if (!gizmoActive)
            return;

        Gizmo.CurrentGizmoType = this switch
        {
            { Moving: true } => CustomGizmo.GizmoType.Move,
            { Rotating: true } => CustomGizmo.GizmoType.Rotate,
            { Scaling: true } => CustomGizmo.GizmoType.Scale,
            _ => CustomGizmo.GizmoType.Rotate,
        };
    }

    protected override void OnDoubleClick()
    {
        base.OnDoubleClick();

        if (Selecting)
            Focus();
    }

    protected override void OnDestroy()
    {
        Destroy(MyGameObject);

        base.OnDestroy();
    }
}
