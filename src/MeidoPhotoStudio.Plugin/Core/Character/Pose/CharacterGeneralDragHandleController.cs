using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class CharacterGeneralDragHandleController : GeneralDragHandleController, ICharacterDragHandleController
{
    private readonly CharacterController character;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TabSelectionController tabSelectionController;

    private bool ikEnabled = true;
    private UpdateTransformMode moveWorldXZ;
    private UpdateTransformMode moveWorldY;
    private UpdateTransformMode rotateLocalXZ;
    private UpdateTransformMode rotateWorldY;
    private UpdateTransformMode rotateLocalY;
    private UpdateTransformMode scale;
    private CharacterSelectMode select;

    public CharacterGeneralDragHandleController(
        DragHandle dragHandle,
        Transform target,
        CharacterController character,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, target)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.character.ChangedTransform += OnTransformChanged;

        TransformBackup = new(Space.World, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    public bool IsCube { get; init; }

    public bool ScalesWithCharacter { get; init; }

    public bool BoneMode { get; set; }

    public override bool Enabled
    {
        get => base.Enabled && IKEnabled;
        set => base.Enabled = value;
    }

    public override bool GizmoEnabled
    {
        get => base.GizmoEnabled && IKEnabled;
        set => base.GizmoEnabled = value;
    }

    public float HandleSize
    {
        get => DragHandle.Size;
        set
        {
            if (!IsCube)
                return;

            DragHandle.Size = value;
        }
    }

    public float GizmoSize
    {
        get => Gizmo.offsetScale;
        set
        {
            if (!IsCube)
                return;

            if (!Gizmo)
                return;

            Gizmo.offsetScale = value;
        }
    }

    public bool IKEnabled
    {
        get => Destroyed
            ? throw new InvalidOperationException("Drag handle controller is destroyed.")
            : IsCube || ikEnabled;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle controller is destroyed.");

            if (IsCube)
                return;

            ikEnabled = value;
            Enabled = ikEnabled;
            GizmoEnabled = ikEnabled;

            CurrentMode.OnModeEnter();
        }
    }

    public override GeneralDragHandleMode<GeneralDragHandleController> Delete =>
        None;

    public override GeneralDragHandleMode<GeneralDragHandleController> MoveWorldXZ =>
        IKEnabled
            ? moveWorldXZ ??= new UpdateTransformMode(
                this, base.MoveWorldXZ, TransformChangeEventArgs.TransformType.Position)
            : None;

    public override GeneralDragHandleMode<GeneralDragHandleController> MoveWorldY =>
        IKEnabled
            ? moveWorldY ??= new UpdateTransformMode(
                this, base.MoveWorldY, TransformChangeEventArgs.TransformType.Position)
            : None;

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateLocalXZ =>
        IKEnabled
            ? rotateLocalXZ ??= new UpdateTransformMode(
                this, base.RotateLocalXZ, TransformChangeEventArgs.TransformType.Rotation)
            : None;

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateWorldY =>
        IKEnabled
            ? rotateWorldY ??= new UpdateTransformMode(
                this, base.RotateWorldY, TransformChangeEventArgs.TransformType.Rotation)
            : None;

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateLocalY =>
        IKEnabled
            ? rotateLocalY ??= new UpdateTransformMode(
                this, base.RotateLocalY, TransformChangeEventArgs.TransformType.Rotation)
            : None;

    public override GeneralDragHandleMode<GeneralDragHandleController> Scale =>
        IKEnabled
            ? scale ??= new UpdateTransformMode(this, base.Scale, TransformChangeEventArgs.TransformType.Scale)
            : None;

    public override GeneralDragHandleMode<GeneralDragHandleController> Select =>
        IKEnabled ? select ??= new CharacterSelectMode(this) : None;

    protected override void OnDestroying() =>
        character.ChangedTransform -= OnTransformChanged;

    private void UpdateCharacterTransform(TransformChangeEventArgs.TransformType type) =>
        character.UpdateTransform(type);

    private void OnTransformChanged(object sender, TransformChangeEventArgs e)
    {
        if (!ScalesWithCharacter)
            return;

        if (e.Type is not TransformChangeEventArgs.TransformType.Scale)
            return;

        DragHandle.Size = character.GameObject.transform.localScale.x;
    }

    private class CharacterSelectMode(CharacterGeneralDragHandleController controller)
        : SelectMode(controller)
    {
        private new CharacterGeneralDragHandleController Controller { get; } = controller;

        public override void OnClicked()
        {
            Controller.selectionController.Select(Controller.character);
            Controller.tabSelectionController.SelectTab(Constants.Window.Pose);
        }

        public override void OnDoubleClicked() =>
            Controller.character.FocusOnBody();
    }

    private class UpdateTransformMode(
        CharacterGeneralDragHandleController controller,
        GeneralDragHandleMode<GeneralDragHandleController> mode,
        TransformChangeEventArgs.TransformType transformType)
        : GeneralDragHandleMode<GeneralDragHandleController>(controller)
    {
        private readonly GeneralDragHandleMode<GeneralDragHandleController> mode = mode;

        private readonly TransformChangeEventArgs.TransformType transformType = transformType;

        private new CharacterGeneralDragHandleController Controller { get; } = controller;

        public override void OnModeEnter() =>
            mode.OnModeEnter();

        public override void OnClicked() =>
            mode.OnClicked();

        public override void OnDragging()
        {
            mode.OnDragging();

            Controller.UpdateCharacterTransform(transformType);
        }

        public override void OnReleased() =>
            mode.OnReleased();

        public override void OnDoubleClicked()
        {
            mode.OnDoubleClicked();

            Controller.UpdateCharacterTransform(transformType);
        }
    }
}
