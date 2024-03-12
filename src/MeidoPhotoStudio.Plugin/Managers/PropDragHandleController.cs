using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropDragHandleController : GeneralDragHandleController
{
    private readonly PropController propController;
    private readonly PropService propService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TabSelectionController tabSelectionController;

    public PropDragHandleController(
        DragHandle dragHandle,
        Transform target,
        CustomGizmo gizmo,
        PropController propController,
        PropService propService,
        SelectionController<PropController> propSelectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, gizmo, target)
    {
        this.propController = propController ?? throw new ArgumentNullException(nameof(propController));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        Gizmo.gameObject.SetActive(false);
        Gizmo.GizmoDrag += OnGizmoDragged;
    }

    public float HandleSize
    {
        get => DragHandle.Size;
        set => DragHandle.Size = value;
    }

    public float GizmoSize
    {
        get => Gizmo.offsetScale;
        set => Gizmo.offsetScale = value;
    }

    protected override void Select()
    {
        base.Select();

        propSelectionController.Select(propController);
        tabSelectionController.SelectTab(Constants.Window.BG2);
    }

    protected override void Delete()
    {
        base.Delete();

        propService.Remove(propController);
    }

    protected override void ResetScale()
    {
        base.ResetScale();

        UpdatePropTransform();
    }

    protected override void ResetRotation()
    {
        base.ResetRotation();

        UpdatePropTransform();
    }

    protected override void ResetPosition()
    {
        base.ResetPosition();

        UpdatePropTransform();
    }

    protected override void OnDragging()
    {
        base.OnDragging();

        if (CurrentDragType is not (DragHandleMode.Select or DragHandleMode.Delete or DragHandleMode.None))
            UpdatePropTransform();
    }

    protected override void OnDoubleClicked()
    {
        base.OnDoubleClicked();

        if (CurrentDragType is DragHandleMode.Select)
            propController.Focus();
    }

    private void UpdatePropTransform() =>
        propController.UpdateTransform();

    private void OnGizmoDragged(object sender, EventArgs e) =>
        propController.UpdateTransform();
}
