using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Gizmo builder.</summary>
public partial class CustomGizmo
{
    public class Builder
    {
        public float LineThickness { get; set; } = 0.25f;

        public GizmoMode Mode { get; set; } = GizmoMode.Local;

        public string Name { get; set; } = "[Gizmo]";

        public float Size { get; set; } = 0.25f;

        public Transform Target { get; set; }

        public GizmoType Type { get; set; } = GizmoType.Rotate;

        public Builder WithLineThickness(float lineThickness)
        {
            LineThickness = lineThickness;

            return this;
        }

        public Builder WithMode(GizmoMode mode)
        {
            Mode = mode;

            return this;
        }

        public Builder WithName(string name)
        {
            Name = name;

            return this;
        }

        public Builder WithSize(float size)
        {
            Size = size;

            return this;
        }

        public Builder WithTarget(Transform target)
        {
            Target = target;

            return this;
        }

        public Builder WithType(GizmoType type)
        {
            Type = type;

            return this;
        }

        public CustomGizmo Build()
        {
            if (!Target)
                throw new InvalidOperationException("Gizmo target cannot be null.");

            var gizmoGameObject = new GameObject(Name);

            gizmoGameObject.transform.SetParent(Target);

            var gizmo = gizmoGameObject.AddComponent<CustomGizmo>();

            gizmo.target = Target;
            gizmo.lineRSelectedThick = LineThickness;
            gizmo.offsetScale = Size;
            gizmo.Mode = Mode;
            gizmo.CurrentGizmoType = Type;

            return gizmo;
        }
    }
}
