using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class UnityExtensions
{
    public static void Deconstruct(this Vector3 vector3, out float x, out float y, out float z) =>
        (x, y, z) = (vector3.x, vector3.y, vector3.z);

    public static void Deconstruct(this Vector2 vector2, out float x, out float y) =>
        (x, y) = (vector2.x, vector2.y);
}
