using System;
using System.Reflection;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class CustomGizmo : GizmoRender
    {
        private static readonly Camera camera = GameMain.Instance.MainCamera.camera;
        private Transform target;
        private readonly FieldInfo beSelectedType = Utility.GetFieldInfo<GizmoRender>("beSelectedType");
        private int SelectedType => (int)beSelectedType.GetValue(this);
        private static readonly FieldInfo is_drag_ = Utility.GetFieldInfo<GizmoRender>("is_drag_");
        public static bool IsDrag
        {
            get => (bool)is_drag_.GetValue(null);
            private set => is_drag_.SetValue(null, value);
        }
        private Vector3 positionOld = Vector3.zero;
        private Vector3 deltaPosition = Vector3.zero;
        private Vector3 deltaLocalPosition = Vector3.zero;
        private Quaternion rotationOld = Quaternion.identity;
        private Quaternion deltaRotation = Quaternion.identity;
        private Quaternion deltaLocalRotation = Quaternion.identity;
        private Vector3 deltaScale = Vector3.zero;
        private Vector3 scaleOld = Vector3.one;
        private GizmoType gizmoTypeOld;
        private GizmoType gizmoType;
        public GizmoType CurrentGizmoType
        {
            get => gizmoType;
            set
            {
                gizmoType = value;
                if (gizmoTypeOld == gizmoType) return;

                gizmoTypeOld = gizmoType;
                eAxis = gizmoType == GizmoType.Move;
                eScal = gizmoType == GizmoType.Scale;
                eRotate = gizmoType == GizmoType.Rotate;
            }
        }
        public bool IsGizmoDrag => GizmoVisible && IsDrag && SelectedType != 0;
        public bool GizmoVisible
        {
            get => Visible;
            set
            {
                if (value && IsDrag) IsDrag = false;
                Visible = value;
            }
        }
        public GizmoMode gizmoMode;
        public event EventHandler GizmoDrag;
        public enum GizmoType { Rotate, Move, Scale }
        public enum GizmoMode { Local, World, Global }

        public static CustomGizmo Make(Transform target, float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
        {
            var gizmoGo = new GameObject($"[MPS Gizmo {target.gameObject.name}]");
            gizmoGo.transform.SetParent(target);

            var gizmo = gizmoGo.AddComponent<CustomGizmo>();
            gizmo.target = target;
            gizmo.lineRSelectedThick = 0.25f;
            gizmo.offsetScale = scale;
            gizmo.gizmoMode = mode;
            gizmo.CurrentGizmoType = GizmoType.Rotate;

            return gizmo;
        }

        public override void Update()
        {
            BeginUpdate();

            base.Update();

            if (IsGizmoDrag) SetTargetTransform();

            SetTransform();

            EndUpdate();
        }

        private void BeginUpdate()
        {
            Quaternion rotation = transform.rotation;
            deltaPosition = transform.position - positionOld;
            deltaRotation = rotation * Quaternion.Inverse(rotationOld);
            deltaLocalPosition = transform.InverseTransformVector(deltaPosition);
            deltaLocalRotation = Quaternion.Inverse(rotationOld) * rotation;
            deltaScale = transform.localScale - scaleOld;
        }

        private void EndUpdate()
        {
            Transform transform = this.transform;
            positionOld = transform.position;
            rotationOld = transform.rotation;
            scaleOld = transform.localScale;
        }

        private void SetTargetTransform()
        {
            bool dragged;

            switch (gizmoMode)
            {
                case GizmoMode.Local:
                    target.position += target.transform.TransformVector(deltaLocalPosition).normalized
                        * deltaLocalPosition.magnitude;
                    target.rotation *= deltaLocalRotation;
                    target.localScale += deltaScale;
                    dragged = deltaLocalRotation != Quaternion.identity || deltaLocalPosition != Vector3.zero
                        || deltaScale != Vector3.zero;
                    break;
                case GizmoMode.World:
                case GizmoMode.Global:
                    target.position += deltaPosition;
                    target.rotation = deltaRotation * target.rotation;
                    dragged = deltaRotation != Quaternion.identity || deltaPosition != Vector3.zero;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            if (dragged) OnGizmoDrag();
        }

        private void SetTransform()
        {
            Transform transform = this.transform;
            transform.position = target.position;
            transform.localScale = Vector3.one;
            transform.rotation = gizmoMode switch
            {
                GizmoMode.Local => target.rotation,
                GizmoMode.World => Quaternion.identity,
                GizmoMode.Global => Quaternion.LookRotation(transform.position - camera.transform.position),
                _ => target.rotation
            };
        }

        private void OnGizmoDrag() => GizmoDrag?.Invoke(this, EventArgs.Empty);
    }
}
