using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class TransformSchema
{
    public const short SchemaVersion = 1;

    public TransformSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public Vector3 Position { get; init; }

    public Quaternion Rotation { get; init; } = Quaternion.identity;

    public Vector3 LocalPosition { get; init; }

    public Quaternion LocalRotation { get; init; } = Quaternion.identity;

    public Vector3 LocalScale { get; init; } = Vector3.one;
}
