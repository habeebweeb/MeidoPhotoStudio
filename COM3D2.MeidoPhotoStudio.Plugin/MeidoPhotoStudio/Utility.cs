using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class Utility
    {
        public static readonly BepInEx.Logging.ManualLogSource Logger
            = BepInEx.Logging.Logger.CreateLogSource(MeidoPhotoStudio.pluginName);
        public enum ModKey
        {
            Control, Shift, Alt
        }

        public static int Wrap(int value, int min, int max)
        {
            max -= 1;
            return value < min ? max : value > max ? min : value;
        }

        public static int GetPix(int num)
        {
            return (int)((1f + (Screen.width / 1280f - 1f) * 0.6f) * num);
        }

        public static float Bound(float value, float left, float right)
        {
            if ((double)left > (double)right) return Mathf.Clamp(value, right, left);
            else return Mathf.Clamp(value, left, right);
        }

        public static Texture2D MakeTex(int width, int height, Color color)
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

        public static FieldInfo GetFieldInfo<T>(string field)
        {
            BindingFlags bindingFlags = BindingFlags.Instance
                | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            return typeof(T).GetField(field, bindingFlags);
        }

        public static TValue GetFieldValue<TType, TValue>(TType instance, string field)
        {
            FieldInfo fieldInfo = GetFieldInfo<TType>(field);
            if (fieldInfo == null || !fieldInfo.IsStatic && instance == null) return default(TValue);
            return (TValue)fieldInfo.GetValue(instance);
        }

        public static void SetFieldValue<TType, TValue>(TType instance, string name, TValue value)
        {
            GetFieldInfo<TType>(name).SetValue(instance, value);
        }

        public static bool GetModKey(ModKey key)
        {
            switch (key)
            {
                case ModKey.Control: return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                case ModKey.Alt: return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                case ModKey.Shift: return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                default: return false;
            }
        }

        public static bool AnyMouseDown()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        }

        public static string ScreenshotFilename()
        {
            string screenShotDir = Path.Combine(
                GameMain.Instance.SerializeStorageManager.StoreDirectoryPath, "ScreenShot"
            );
            if (!Directory.Exists(screenShotDir))
            {
                Directory.CreateDirectory(screenShotDir);
            }
            return Path.Combine(screenShotDir, $"img{DateTime.Now:yyyyMMddHHmmss}.png");
        }

        public static void ShowMouseExposition(string text, float time = 2f)
        {
            MouseExposition mouseExposition = MouseExposition.GetObject();
            mouseExposition.SetText(text, time);
        }

        public static string HandItemToOdogu(string menu)
        {
            menu = menu.Substring(menu.IndexOf('_') + 1);
            menu = menu.Substring(0, menu.IndexOf("_i_.menu"));
            menu = $"odogu_{menu}";
            return menu;
        }

        public static void FixGameObjectScale(GameObject go)
        {
            Vector3 scale = go.transform.localScale;
            float largest = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
            go.transform.localScale = Vector3.one * (float)Math.Round(largest, 3);
        }
    }
}
