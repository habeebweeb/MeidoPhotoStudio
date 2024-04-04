using System;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

// TODO: 🤮 This and the Constants class are a huge disgrace.
public static class Utility
{
    internal static readonly GameObject MousePositionGameObject;
    internal static readonly MousePosition MousePositionValue;

    private static readonly BepInEx.Logging.ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource(Plugin.PluginName);

    static Utility()
    {
        MousePositionGameObject = new();
        MousePositionValue = MousePositionGameObject.AddComponent<MousePosition>();
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

    public static bool SeekPngEnd(Stream stream)
    {
        var buffer = new byte[8];

        var pngHeader = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

        stream.Read(buffer, 0, 8);

        if (!buffer.SequenceEqual(pngHeader))
            return false;

        var pngEnd = System.Text.Encoding.ASCII.GetBytes("IEND");

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
        while (!buffer.SequenceEqual(pngEnd));

        return true;
    }
}
