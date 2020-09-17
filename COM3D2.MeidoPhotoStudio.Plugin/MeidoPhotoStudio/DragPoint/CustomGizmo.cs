using System;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class CustomGizmo : GizmoRender
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
                if (gizmoTypeOld != gizmoType) SetGizmoType(gizmoType);
                gizmoTypeOld = gizmoType;
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
        public enum GizmoType
        {
            Rotate, Move, Scale, None
        }
        public enum GizmoMode
        {
            Local, World, Global
        }

        public static CustomGizmo Make(Transform target, float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
        {
            GameObject gizmoGo = new GameObject("[MPS Gizmo]");
            gizmoGo.transform.SetParent(target);

            CustomGizmo gizmo = gizmoGo.AddComponent<CustomGizmo>();
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
            deltaPosition = transform.position - positionOld;
            deltaRotation = transform.rotation * Quaternion.Inverse(rotationOld);
            deltaLocalPosition = transform.InverseTransformVector(deltaPosition);
            deltaLocalRotation = Quaternion.Inverse(rotationOld) * transform.rotation;
            deltaScale = transform.localScale - scaleOld;
        }

        private void EndUpdate()
        {
            positionOld = transform.position;
            rotationOld = transform.rotation;
            scaleOld = transform.localScale;
        }

        private void SetTargetTransform()
        {
            bool dragged = false;
            switch (gizmoMode)
            {
                case GizmoMode.Local:
                    target.transform.position += target.transform.TransformVector(deltaLocalPosition).normalized
                        * deltaLocalPosition.magnitude;
                    target.transform.rotation *= deltaLocalRotation;
                    target.transform.localScale += deltaScale;
                    if (deltaLocalRotation != Quaternion.identity || deltaLocalPosition != Vector3.zero
                        || deltaScale != Vector3.zero
                    ) dragged = true;
                    break;
                case GizmoMode.World:
                case GizmoMode.Global:
                    target.transform.position += deltaPosition;
                    target.transform.rotation = deltaRotation * target.transform.rotation;
                    if (deltaRotation != Quaternion.identity || deltaPosition != Vector3.zero) dragged = true;
                    break;
            }
            if (dragged) OnGizmoDrag();
        }

        private void SetTransform()
        {
            switch (gizmoMode)
            {
                case GizmoMode.Local:
                    transform.position = target.transform.position;
                    transform.rotation = target.transform.rotation;
                    break;
                case GizmoMode.World:
                    transform.position = target.transform.position;
                    transform.rotation = Quaternion.identity;
                    break;
                case GizmoMode.Global:
                    transform.position = target.transform.position;
                    transform.rotation = Quaternion.LookRotation(
                        transform.position - camera.transform.position, transform.up
                    );
                    break;
            }
            transform.localScale = Vector3.one;
        }

        private void SetGizmoType(GizmoType gizmoType)
        {
            switch (gizmoType)
            {
                case GizmoType.Move:
                    eAxis = true;
                    eRotate = false;
                    eScal = false;
                    break;
                case GizmoType.Rotate:
                    eAxis = false;
                    eRotate = true;
                    eScal = false;
                    break;
                case GizmoType.Scale:
                    eAxis = false;
                    eRotate = false;
                    eScal = true;
                    break;
                case GizmoType.None:
                    eAxis = false;
                    eRotate = false;
                    eScal = false;
                    break;
            }
        }

        private void OnGizmoDrag()
        {
            GizmoDrag?.Invoke(this, EventArgs.Empty);
        }
    }
}
