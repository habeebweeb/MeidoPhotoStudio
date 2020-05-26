using System.Reflection;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public static class Utility
    {
        public enum ModKey
        {
            Control, Shift, Alt
        }
        internal static int Wrap(int value, int min, int max)
        {
            max -= 1;
            return value < min ? max : value > max ? min : value;
        }
        internal static int GetPix(int num)
        {
            return (int)((1f + (Screen.width / 1280f - 1f) * 0.6f) * num);
        }
        internal static Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            Texture2D texture2D = new Texture2D(width, height);
            texture2D.SetPixels(colors);
            texture2D.Apply();
            return texture2D;
        }
        internal static FieldInfo GetFieldInfo<T>(string field)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            return typeof(T).GetField(field, bindingFlags);
        }
        internal static TValue GetFieldValue<TType, TValue>(TType instance, string field)
        {
            FieldInfo fieldInfo = GetFieldInfo<TType>(field);
            if (fieldInfo == null || !fieldInfo.IsStatic && instance == null) return default(TValue);
            return (TValue)fieldInfo.GetValue(instance);
        }
        internal static void SetFieldValue<TType, TValue>(TType instance, string name, TValue value)
        {
            FieldInfo fieldInfo = GetFieldInfo<TType>(name);
            fieldInfo.SetValue(instance, value);
        }

        internal static bool GetModKey(ModKey key)
        {
            switch (key)
            {
                case ModKey.Control: return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                case ModKey.Alt: return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                case ModKey.Shift: return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                default: return false;
            }
        }
        internal static bool AnyMouseDown()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        }
    }
}
