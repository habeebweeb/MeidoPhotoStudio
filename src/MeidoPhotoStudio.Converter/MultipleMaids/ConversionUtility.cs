using UnityEngine;

namespace MeidoPhotoStudio.Converter.MultipleMaids
{
    internal static class ConversionUtility
    {
        public static Quaternion ParseEulerAngle(string euler)
        {
            var data = euler.Split(',');

            return Quaternion.Euler(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
        }

        public static Vector3 ParseVector3(string vector3)
        {
            var data = vector3.Split(',');
            return new(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
        }
    }
}
