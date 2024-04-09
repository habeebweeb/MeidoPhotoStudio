namespace MeidoPhotoStudio.Plugin.Framework;

public readonly record struct TransformBackup(Space Space, Vector3 Position, Quaternion Rotation, Vector3 LocalScale)
{
    public TransformBackup(Transform transform, Space space = Space.World)
        : this(
            space,
            space is Space.World ? transform.position : transform.localPosition,
            space is Space.World ? transform.rotation : transform.localRotation,
            transform.localScale)
    {
    }

    public void ApplyRotation(Transform transform)
    {
        if (Space is Space.World)
            transform.rotation = Rotation;
        else
            transform.localRotation = Rotation;
    }

    public void ApplyPosition(Transform transform)
    {
        if (Space is Space.World)
            transform.position = Position;
        else
            transform.localPosition = Position;
    }

    public void ApplyScale(Transform transform) =>
        transform.localScale = LocalScale;

    public void Apply(Transform transform)
    {
        if (Space is Space.World)
        {
            transform.SetPositionAndRotation(Position, Rotation);
        }
        else
        {
            transform.localPosition = Position;
            transform.localRotation = Rotation;
        }

        transform.localScale = LocalScale;
    }
}
