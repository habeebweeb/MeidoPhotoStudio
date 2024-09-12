namespace MeidoPhotoStudio.Plugin.Framework.UIGizmo;

/// <summary>Drag handle builder.</summary>
public partial class DragHandle
{
    public class Builder
    {
        private static readonly Material DragHandleMaterial = new(Shader.Find("CM3D2/Trans_AbsoluteFront"));

        private static GameObject dragHandleParent;

        public Color Color { get; set; } = new(0f, 0f, 0f, 0.4f);

        public bool ConstantSize { get; set; }

        public string Name { get; set; } = "[Drag Handle]";

        public Func<Vector3> PositionDelegate { get; set; } = DefaultPositionDelegate;

        public int Priority { get; set; }

        public Func<Quaternion> RotationDelegate { get; set; } = DefaultRotationDelegate;

        public Vector3 Scale { get; set; } = Vector3.one;

        public PrimitiveType Shape { get; set; } = PrimitiveType.Cube;

        public float Size { get; set; } = 1f;

        public Transform Target { get; set; }

        public bool Visible { get; set; } = true;

        private static GameObject DragHandleParent => dragHandleParent
            ? dragHandleParent
            : dragHandleParent = new("[MPS Drag Handle Parent]");

        public Builder WithColor(Color color)
        {
            Color = color;

            return this;
        }

        public Builder WithConstantSize(bool constantSize)
        {
            ConstantSize = constantSize;

            return this;
        }

        public Builder WithName(string name)
        {
            Name = name;

            return this;
        }

        public Builder WithPositionDelegate(Func<Vector3> positionDelegate)
        {
            PositionDelegate = positionDelegate;

            return this;
        }

        public Builder WithPriority(int priority)
        {
            Priority = priority;

            return this;
        }

        public Builder WithRotationDelegate(Func<Quaternion> rotationDelegate)
        {
            RotationDelegate = rotationDelegate;

            return this;
        }

        public Builder WithScale(Vector3 scale)
        {
            Scale = scale;

            return this;
        }

        public Builder WithShape(PrimitiveType shape)
        {
            Shape = shape;

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

        public Builder WithVisible(bool visible)
        {
            Visible = visible;

            return this;
        }

        public DragHandle Build()
        {
            var handle = GameObject.CreatePrimitive(Shape);

            handle.layer = DragHandleLayer;
            handle.name = Name;

            var transform = handle.transform;

            transform.SetParent(DragHandleParent.transform, true);
            transform.localScale = Scale * Size;

            var dragHandle = handle.AddComponent<DragHandle>();

            dragHandle.Target = Target;
            dragHandle.Size = Size;
            dragHandle.Scale = Scale;
            dragHandle.ConstantSize = ConstantSize;
            dragHandle.PositionDelegate = PositionDelegate;
            dragHandle.Priority = Priority;
            dragHandle.RotationDelegate = RotationDelegate;
            dragHandle.Visible = Visible;

            var renderer = dragHandle.Renderer;

            renderer.material = DragHandleMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material.color = Color;

            return dragHandle;
        }

        internal static void DestroyParent()
        {
            if (!dragHandleParent)
                return;

            Destroy(dragHandleParent);
        }
    }
}
