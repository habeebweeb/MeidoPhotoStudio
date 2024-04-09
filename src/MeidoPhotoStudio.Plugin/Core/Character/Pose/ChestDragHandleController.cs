using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestDragHandleController : CharacterIKDragHandleController
{
    private readonly bool left;

    private NoneMode none;
    private DragMode drag;
    private GizmoRotateMode rotateGizmo;

    public ChestDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        Transform bone,
        Transform ikTarget)
        : base(dragHandle, gizmo, characterController, bone, ikTarget)
    {
        left = bone.name.StartsWith("Mune_L");

        Chain = [Bone.parent, Bone];

        Gizmo.GizmoDrag += OnGizmoDragged;
    }

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode RotateGizmo =>
        rotateGizmo ??= new GizmoRotateMode(this);

    protected override Transform[] Chain { get; }

    private void SetMuneEnabled(bool enabled)
    {
        if (left)
            IKController.MuneLEnabled = enabled;
        else
            IKController.MuneREnabled = enabled;
    }

    private void OnGizmoDragged(object sender, EventArgs e) =>
        SetMuneEnabled(false);

    private class NoneMode(ChestDragHandleController controller)
        : DragHandleMode
    {
        private readonly ChestDragHandleController controller = controller
            ?? throw new ArgumentNullException(nameof(controller));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }
    }

    private new class DragMode(ChestDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        private readonly ChestDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Visible = false;
        }

        public override void OnClicked()
        {
            controller.SetMuneEnabled(false);

            base.OnClicked();
        }

        public override void OnDoubleClicked()
        {
            controller.SetMuneEnabled(true);

            base.OnDoubleClicked();
        }
    }

    private class GizmoRotateMode(ChestDragHandleController controller)
        : DragHandleMode
    {
        private readonly ChestDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = true;
        }
    }
}
