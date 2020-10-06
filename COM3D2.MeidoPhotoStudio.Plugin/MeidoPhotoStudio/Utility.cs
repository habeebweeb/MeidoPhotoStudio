using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public static class Utility
    {
        internal static readonly byte[] pngHeader = { 137, 80, 78, 71, 13, 10, 26, 10 };
        internal static readonly byte[] pngEnd = System.Text.Encoding.ASCII.GetBytes("IEND");
        internal static readonly Regex guidRegEx = new Regex(
            @"^[a-f0-9]{8}(\-[a-f0-9]{4}){3}\-[a-f0-9]{12}$", RegexOptions.IgnoreCase
        );
        internal static readonly GameObject mousePositionGo;
        internal static readonly MousePosition mousePosition;
        public static readonly BepInEx.Logging.ManualLogSource Logger
            = BepInEx.Logging.Logger.CreateLogSource(MeidoPhotoStudio.pluginName);
        public enum ModKey
        {
            Control, Shift, Alt
        }
        public static string Timestamp => $"{DateTime.Now:yyyyMMddHHmmss}";
        public static Vector3 MousePosition => mousePosition.Position;

        static Utility()
        {
            mousePositionGo = new GameObject();
            mousePosition = mousePositionGo.AddComponent<MousePosition>();
        }

        public static void LogInfo(object data) => Logger.LogInfo(data);

        public static void LogMessage(object data) => Logger.LogMessage(data);

        public static void LogWarning(object data) => Logger.LogWarning(data);

        public static void LogError(object data) => Logger.LogError(data);

        public static void LogDebug(object data) => Logger.LogDebug(data);

        public static int Wrap(int value, int min, int max)
        {
            max--;
            return value < min ? max : value > max ? min : value;
        }

        public static int GetPix(int num) => (int)((1f + (((Screen.width / 1280f) - 1f) * 0.6f)) * num);

        public static float Bound(float value, float left, float right)
        {
            return left > (double)right ? Mathf.Clamp(value, right, left) : Mathf.Clamp(value, left, right);
        }

        public static int Bound(int value, int left, int right)
        {
            return left > right ? Mathf.Clamp(value, right, left) : Mathf.Clamp(value, left, right);
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
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            return typeof(T).GetField(field, bindingFlags);
        }

        public static TValue GetFieldValue<TType, TValue>(TType instance, string field)
        {
            FieldInfo fieldInfo = GetFieldInfo<TType>(field);
            if (fieldInfo == null || (!fieldInfo.IsStatic && instance == null)) return default;
            return (TValue)fieldInfo.GetValue(instance);
        }

        public static void SetFieldValue<TType, TValue>(TType instance, string name, TValue value)
        {
            GetFieldInfo<TType>(name).SetValue(instance, value);
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
            if (!Directory.Exists(screenShotDir)) Directory.CreateDirectory(screenShotDir);
            return Path.Combine(screenShotDir, $"img{Timestamp}.png");
        }

        public static string TempScreenshotFilename()
        {
            return Path.Combine(Path.GetTempPath(), $"cm3d2_{Guid.NewGuid()}.png");
        }

        public static void ShowMouseExposition(string text, float time = 2f)
        {
            MouseExposition mouseExposition = MouseExposition.GetObject();
            mouseExposition.SetText(text, time);
        }

        public static bool IsGuidString(string guid)
        {
            if (string.IsNullOrEmpty(guid) || guid.Length != 36) return false;
            return guidRegEx.IsMatch(guid);
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

        public static string SanitizePathPortion(string path)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            path = path.Trim();
            path = string.Join("_", path.Split(invalid)).Replace(".", "").Trim('_');
            return path;
        }

        public static string GP01FbFaceHash(TMorph face, string hash)
        {
            if ((face.bodyskin.PartsVersion >= 120) && (hash != "eyeclose3") && hash.StartsWith("eyeclose"))
            {
                if (hash == "eyeclose") hash += '1';
                hash += TMorph.crcFaceTypesStr[(int)face.GetFaceTypeGP01FB()];
            }
            return hash;
        }

        public static void ResizeToFit(Texture2D texture, int maxWidth, int maxHeight)
        {
            int width = texture.width;
            int height = texture.height;
            if (width != maxWidth || height != maxHeight)
            {
                float scale = Mathf.Min(maxWidth / (float)width, maxHeight / (float)height);
                width = Mathf.RoundToInt(width * scale);
                height = Mathf.RoundToInt(height * scale);
                TextureScale.Bilinear(texture, width, height);
            }
        }

        public static bool BytesEqual(byte[] buffer, byte[] other)
        {
            if (buffer.Length != other.Length) return false;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != other[i]) return false;
            }
            return true;
        }

        public static bool IsPngFile(Stream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            stream.Position = 0L;
            return BytesEqual(buffer, pngHeader);
        }

        public static bool SeekPngEnd(Stream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            if (!BytesEqual(buffer, pngHeader)) return false;
            buffer = new byte[4];
            do
            {
                stream.Read(buffer, 0, 4);
                if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
                uint length = BitConverter.ToUInt32(buffer, 0);
                stream.Read(buffer, 0, 4);
                stream.Seek(length + 4L, SeekOrigin.Current);
            } while (!BytesEqual(buffer, pngEnd));
            return true;
        }

        public static void WriteToFile(string name, System.Collections.Generic.IEnumerable<string> list)
        {
            if (Path.GetExtension(name) != ".txt") name += ".txt";
            File.WriteAllLines(Path.Combine(Constants.configPath, name), list.ToArray());
        }
    }

    public class MousePosition : MonoBehaviour
    {
        private Vector3 mousePosition;
        public Vector3 Position => mousePosition;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            mousePosition = Input.mousePosition;
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                mousePosition.x += Input.GetAxis("Mouse X") * 20;
                mousePosition.y += Input.GetAxis("Mouse Y") * 20;
            }
            else mousePosition = Input.mousePosition;
        }
    }

    public static class BinaryExtensions
    {
        public static string ReadNullableString(this BinaryReader binaryReader)
        {
            return binaryReader.ReadBoolean() ? binaryReader.ReadString() : null;
        }

        public static void WriteNullableString(this BinaryWriter binaryWriter, string str)
        {
            binaryWriter.Write(str != null);
            if (str != null) binaryWriter.Write(str);
        }

        public static void WriteVector3(this BinaryWriter binaryWriter, Vector3 vector3)
        {
            binaryWriter.Write(vector3.x);
            binaryWriter.Write(vector3.y);
            binaryWriter.Write(vector3.z);
        }

        public static Vector3 ReadVector3(this BinaryReader binaryReader)
        {
            return new Vector3(
                binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()
            );
        }

        public static void WriteQuaternion(this BinaryWriter binaryWriter, Quaternion quaternion)
        {
            binaryWriter.Write(quaternion.x);
            binaryWriter.Write(quaternion.y);
            binaryWriter.Write(quaternion.z);
            binaryWriter.Write(quaternion.w);
        }

        public static Quaternion ReadQuaternion(this BinaryReader binaryReader)
        {
            return new Quaternion
            (
                binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle(), binaryReader.ReadSingle()
            );
        }

        public static void WriteColour(this BinaryWriter binaryWriter, Color colour)
        {
            binaryWriter.Write(colour.r);
            binaryWriter.Write(colour.g);
            binaryWriter.Write(colour.b);
            binaryWriter.Write(colour.a);
        }

        public static Color ReadColour(this BinaryReader binaryReader)
        {
            return new Color
            (
                binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle(), binaryReader.ReadSingle()
            );
        }
    }
}
