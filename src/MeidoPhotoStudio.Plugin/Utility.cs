using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public static class Utility
{
    internal static readonly byte[] PngHeader = { 137, 80, 78, 71, 13, 10, 26, 10 };
    internal static readonly byte[] PngEnd = System.Text.Encoding.ASCII.GetBytes("IEND");
    internal static readonly Regex GuidRegEx =
        new(@"^[a-f0-9]{8}(\-[a-f0-9]{4}){3}\-[a-f0-9]{12}$", RegexOptions.IgnoreCase);

    internal static readonly GameObject MousePositionGameObject;
    internal static readonly MousePosition MousePositionValue;

    private const BindingFlags ReflectionFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    private static readonly BepInEx.Logging.ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource(MeidoPhotoStudio.PluginName);

    static Utility()
    {
        MousePositionGameObject = new();
        MousePositionValue = MousePositionGameObject.AddComponent<MousePosition>();
    }

    public enum ModKey
    {
        Control,
        Shift,
        Alt,
    }

    public static string Timestamp =>
        $"{DateTime.Now:yyyyMMddHHmmss}";

    public static Vector3 MousePosition =>
        MousePositionValue.Position;

    public static void LogInfo(object data) =>
        Logger.LogInfo(data);

    public static void LogMessage(object data) =>
        Logger.LogMessage(data);

    public static void LogWarning(object data) =>
        Logger.LogWarning(data);

    public static void LogError(object data) =>
        Logger.LogError(data);

    public static void LogDebug(object data) =>
        Logger.LogDebug(data);

    public static int Wrap(int value, int min, int max)
    {
        max--;

        return value < min ? max : value > max ? min : value;
    }

    public static int GetPix(int num) =>
        (int)((1f + (Screen.width / 1280f - 1f) * 0.6f) * num);

    public static float Bound(float value, float left, float right) =>
        left > (double)right ? Mathf.Clamp(value, right, left) : Mathf.Clamp(value, left, right);

    public static int Bound(int value, int left, int right) =>
        left > right ? Mathf.Clamp(value, right, left) : Mathf.Clamp(value, left, right);

    public static Texture2D MakeTex(int width, int height, Color color)
    {
        var colors = new Color32[width * height];

        for (var i = 0; i < colors.Length; i++)
            colors[i] = color;

        var texture2D = new Texture2D(width, height);

        texture2D.SetPixels32(colors);
        texture2D.Apply();

        return texture2D;
    }

    public static FieldInfo GetFieldInfo<T>(string field) =>
        typeof(T).GetField(field, ReflectionFlags);

    public static TValue GetFieldValue<TType, TValue>(TType instance, string field)
    {
        var fieldInfo = GetFieldInfo<TType>(field);

        return fieldInfo is null || !fieldInfo.IsStatic && instance == null
            ? default
            : (TValue)fieldInfo.GetValue(instance);
    }

    public static void SetFieldValue<TType, TValue>(TType instance, string name, TValue value) =>
        GetFieldInfo<TType>(name).SetValue(instance, value);

    public static PropertyInfo GetPropertyInfo<T>(string field) =>
        typeof(T).GetProperty(field, ReflectionFlags);

    public static TValue GetPropertyValue<TType, TValue>(TType instance, string property)
    {
        var propertyInfo = GetPropertyInfo<TType>(property);

        return propertyInfo is null
            ? default
            : (TValue)propertyInfo.GetValue(instance, null);
    }

    public static void SetPropertyValue<TType, TValue>(TType instance, string name, TValue value) =>
        GetPropertyInfo<TType>(name).SetValue(instance, value, null);

    public static bool AnyMouseDown() =>
        Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);

    public static string ScreenshotFilename()
    {
        var screenShotDir = Path.Combine(GameMain.Instance.SerializeStorageManager.StoreDirectoryPath, "ScreenShot");

        if (!Directory.Exists(screenShotDir))
            Directory.CreateDirectory(screenShotDir);

        return Path.Combine(screenShotDir, $"img{Timestamp}.png");
    }

    public static string TempScreenshotFilename() =>
        Path.Combine(Path.GetTempPath(), $"cm3d2_{Guid.NewGuid()}.png");

    public static void ShowMouseExposition(string text, float time = 2f)
    {
        var mouseExposition = MouseExposition.GetObject();

        mouseExposition.SetText(text, time);
    }

    public static bool IsGuidString(string guid) =>
        !string.IsNullOrEmpty(guid) && guid.Length is 36 && GuidRegEx.IsMatch(guid);

    public static string HandItemToOdogu(string menu)
    {
        menu = menu.Substring(menu.IndexOf('_') + 1);
        menu = menu.Substring(0, menu.IndexOf("_i_.menu", StringComparison.OrdinalIgnoreCase));
        menu = $"odogu_{menu}";

        return menu;
    }

    public static string SanitizePathPortion(string path)
    {
        var invalid = Path.GetInvalidFileNameChars();

        path = path.Trim();
        path = string.Join("_", path.Split(invalid)).Replace(".", string.Empty).Trim('_');

        return path;
    }

    public static string GP01FbFaceHash(TMorph face, string hash)
    {
        if (face.bodyskin.PartsVersion < 120 || hash is "eyeclose3" || !hash.StartsWith("eyeclose"))
            return hash;

        if (hash is "eyeclose")
            hash += '1';

        hash += TMorph.crcFaceTypesStr[(int)face.GetFaceTypeGP01FB()];

        return hash;
    }

    public static void ResizeToFit(Texture2D texture, int maxWidth, int maxHeight)
    {
        var width = texture.width;
        var height = texture.height;

        if (width == maxWidth && height == maxHeight)
            return;

        var scale = Mathf.Min(maxWidth / (float)width, maxHeight / (float)height);

        width = Mathf.RoundToInt(width * scale);
        height = Mathf.RoundToInt(height * scale);
        TextureScale.Bilinear(texture, width, height);
    }

    public static bool BytesEqual(byte[] buffer, byte[] other)
    {
        if (buffer.Length != other.Length)
            return false;

        for (var i = 0; i < buffer.Length; i++)
            if (buffer[i] != other[i])
                return false;

        return true;
    }

    public static bool IsPngFile(Stream stream)
    {
        var buffer = new byte[8];

        stream.Read(buffer, 0, 8);

        return BytesEqual(buffer, PngHeader);
    }

    public static bool SeekPngEnd(Stream stream)
    {
        var buffer = new byte[8];

        stream.Read(buffer, 0, 8);

        if (!BytesEqual(buffer, PngHeader))
            return false;

        buffer = new byte[4];

        do
        {
            stream.Read(buffer, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            var length = BitConverter.ToUInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            stream.Seek(length + 4L, SeekOrigin.Current);
        }
        while (!BytesEqual(buffer, PngEnd));

        return true;
    }

    public static void WriteToFile(string name, IEnumerable<string> list)
    {
        if (Path.GetExtension(name) is not ".txt")
            name += ".txt";

        File.WriteAllLines(Path.Combine(Constants.ConfigPath, name), list.ToArray());
    }

    public static void WriteToFile(string name, byte[] data) =>
        File.WriteAllBytes(Path.Combine(Constants.ConfigPath, name), data);
}
