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

        /// <summary>
        /// Checks if the string has 3 euler angle components delimited by commas before parsing
        /// </summary>
        /// <param name="euler">Euler angle string in the form "x,y,z"</param>
        /// <param name="result">Resulting angle as a <c>Quaternion</c></param>
        /// <returns>Whether or not the euler string can be safely parsed</returns>
        public static bool TryParseEulerAngle(string euler, out Quaternion result)
        {
            result = Quaternion.identity;

            var data = euler.Split(',');

            if (data.Length != 3) return false;

            try { result = Quaternion.Euler(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])); }
            catch { return false; }

            return true;
        }
    }
}
