using System;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class CustomGizmo : GizmoRender
    {
        private Transform target;
        private FieldInfo beSelectedType = Utility.GetFieldInfo<GizmoRender>("beSelectedType");
        private int SelectedType => (int)beSelectedType.GetValue(this);
        private static FieldInfo is_drag_ = Utility.GetFieldInfo<GizmoRender>("is_drag_");
        private static bool IsDrag => (bool)is_drag_.GetValue(null);
        private Vector3 positionOld = Vector3.zero;
        private Vector3 deltaPosition = Vector3.zero;
        private Vector3 localPositionOld = Vector3.zero;
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
        public bool IsGizmoDrag => IsDrag && SelectedType != 0;
        public GizmoMode gizmoMode = GizmoMode.Local;
        public event EventHandler GizmoDrag;
        public enum GizmoType
        {
            Rotate, Move, Scale, None
        }
        public enum GizmoMode
        {
            Local, World, Global
        }

        public static GameObject MakeGizmo(Transform target, float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
        {
            GameObject gizmoGo = new GameObject();
            gizmoGo.transform.SetParent(target);

            CustomGizmo gizmo = gizmoGo.AddComponent<CustomGizmo>();
            gizmo.target = target;
            gizmo.lineRSelectedThick = 0.25f;
            gizmo.offsetScale = scale;
            gizmo.gizmoMode = mode;
            gizmo.CurrentGizmoType = GizmoType.Rotate;

            return gizmoGo;
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
            switch (gizmoMode)
            {
                case GizmoMode.Local:
                    target.transform.position += target.transform.TransformVector(deltaLocalPosition).normalized
                        * deltaLocalPosition.magnitude;
                    target.transform.rotation = target.transform.rotation * deltaLocalRotation;
                    target.transform.localScale += deltaScale;
                    if (deltaLocalRotation != Quaternion.identity || deltaLocalPosition != Vector3.zero
                        || deltaScale != Vector3.zero
                    )
                    {
                        OnGizmoDrag();
                    }
                    break;
                case GizmoMode.World:
                case GizmoMode.Global:
                    target.transform.position += deltaPosition;
                    target.transform.rotation = deltaRotation * target.transform.rotation;
                    if (deltaRotation != Quaternion.identity || deltaPosition != Vector3.zero) OnGizmoDrag();
                    break;

            }
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
                        transform.position - Camera.main.transform.position, transform.up
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
                    this.eAxis = true;
                    this.eRotate = false;
                    this.eScal = false;
                    break;
                case GizmoType.Rotate:
                    this.eAxis = false;
                    this.eRotate = true;
                    this.eScal = false;
                    break;
                case GizmoType.Scale:
                    this.eAxis = false;
                    this.eRotate = false;
                    this.eScal = true;
                    break;
                case GizmoType.None:
                    this.eAxis = false;
                    this.eRotate = false;
                    this.eScal = false;
                    break;
            }
        }

        private void OnGizmoDrag()
        {
            GizmoDrag?.Invoke(this, EventArgs.Empty);
        }

        private void OnEnable()
        {
            if (target != null)
            {
                SetTransform();
            }
        }
    }
}
