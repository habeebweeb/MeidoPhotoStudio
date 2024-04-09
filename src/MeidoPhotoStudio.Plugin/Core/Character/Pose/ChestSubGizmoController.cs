using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestSubGizmoController : CharacterDragHandleController
{
    private readonly bool left;

    private NoneMode none;
    private RotateMode rotate;

    public ChestSubGizmoController(CustomGizmo gizmo, CharacterController characterController, Transform bone)
        : base(gizmo, characterController)
    {
        left = bone.name.StartsWith("Mune_L");

        Gizmo.GizmoDrag += OnGizmoDragged;
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    private void SetMuneEnabled(bool enabled)
    {
        if (left)
            IKController.MuneLEnabled = enabled;
        else
            IKController.MuneREnabled = enabled;
    }

    private void OnGizmoDragged(object sender, EventArgs e) =>
        SetMuneEnabled(false);

    private class NoneMode(ChestSubGizmoController controller)
        : DragHandleMode
    {
        private readonly ChestSubGizmoController controller = controller;

        public override void OnModeEnter() =>
            controller.GizmoActive = false;
    }

    private class RotateMode(ChestSubGizmoController controller)
        : DragHandleMode
    {
        private readonly ChestSubGizmoController controller = controller;

        public override void OnModeEnter() =>
            controller.GizmoActive = true;
    }
}
