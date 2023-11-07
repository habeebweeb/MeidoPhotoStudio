using UnityEngine;

namespace MeidoPhotoStudio.Converter.Serialization;

public class TransformDTO
{
    public TransformDTO()
    {
    }

    public TransformDTO(Transform transform)
    {
        Position = transform.position;
        LocalPosition = transform.localPosition;
        Rotation = transform.rotation;
        LocalRotation = transform.localRotation;
        LocalScale = transform.localScale;
    }

    public Vector3 Position { get; set; }

    public Vector3 LocalPosition { get; set; }

    public Quaternion Rotation { get; set; } = Quaternion.identity;

    public Quaternion LocalRotation { get; set; } = Quaternion.identity;

    public Vector3 LocalScale { get; set; } = Vector3.one;
}
