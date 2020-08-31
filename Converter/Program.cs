using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zlib;
using ExIni;

namespace Converter
{
    public class Program
    {
        private static StreamWriter writer;
        public static void Main(string[] args)
        {
            string writerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            IniFile mmIniFile = IniFile.FromFile(args[0]);

            IniSection sceneSection = mmIniFile.GetSection("scene");

            using (writer = new StreamWriter(writerPath))
            {
                if (sceneSection != null)
                {
                    foreach (IniKey key in sceneSection.Keys)
                    {
                        ProcessScene(key);
                    }
                }
            }
        }

        public static void ProcessScene(IniKey sceneKey)
        {
            if (sceneKey.Key.StartsWith("ss")) return;

            string sceneData = sceneKey.Value;

            if (string.IsNullOrEmpty(sceneData)) return;

            writer.WriteLine($"Deserialize {sceneKey.Key}");

            string[] strArray1 = sceneData.Split('_');
            string[] strArray2 = strArray1[1].Split(';');
            string[] strArray3 = strArray1[0].Split(',');
            string[] strArray4 = null;
            string[] strArray5 = null;
            string[] strArray6 = null;
            string[] strArray7 = null;

            if (strArray1.Length >= 5)
            {
                strArray4 = strArray1[2].Split(',');
                strArray5 = strArray1[3].Split(';');
                strArray6 = strArray1[4].Split(';');
            }

            if (strArray1.Length >= 6)
            {
                strArray7 = strArray1[5].Split(';');
            }

            // Environment

            int bgIndex;

            string bgAsset = "Theater";

            if (!int.TryParse(strArray3[2], out bgIndex))
            {
                bgAsset = strArray3[2].Replace(" ", "_");
            }
            else
            {
                writer.WriteLine($"No BG string: {bgIndex}");
            }

            Quaternion bgRotation = Quaternion.Euler(
                float.Parse(strArray3[3]), float.Parse(strArray3[4]), float.Parse(strArray3[5])
            );

            Vector3 bgPosition = new Vector3(
                float.Parse(strArray3[6]), float.Parse(strArray3[7]), float.Parse(strArray3[8])
            );

            Vector3 bgLocalScale = new Vector3(
                float.Parse(strArray3[9]), float.Parse(strArray3[10]), float.Parse(strArray3[11])
            );

            if (strArray3.Length > 16)
            {
                Vector3 cameraTargetPos = new Vector3(
                    float.Parse(strArray3[27]), float.Parse(strArray3[28]), float.Parse(strArray3[29])
                );

                float cameraDistance = float.Parse(strArray3[30]);

                Quaternion cameraRotation = Quaternion.Euler(
                    float.Parse(strArray3[31]), float.Parse(strArray3[32]), float.Parse(strArray3[33])
                );
            }

            // Lights

            if (strArray3.Length > 16)
            {
                // Main Light
                /*
                    0 = Directional
                    1 = Spot
                    2 = Point
                    3 = Directional (Colour Mode)
                */
                int lightType = int.Parse(strArray3[17]);
                Color lightColor = new Color(
                    float.Parse(strArray3[18]), float.Parse(strArray3[19]), float.Parse(strArray3[20]), 1f
                );

                Quaternion lightRotation = Quaternion.Euler(
                    float.Parse(strArray3[21]), float.Parse(strArray3[22]), float.Parse(strArray3[23])
                );

                // MM uses spotAngle for both range and spotAngle based on which light type is used
                // TODO: assign value from spot angle appropriately 
                float intensity = float.Parse(strArray3[24]);
                float spotAngle = float.Parse(strArray3[25]);
                float range = float.Parse(strArray3[26]);
                float shadowStrength = 0.098f;
                if (strArray4 != null) shadowStrength = float.Parse(strArray4[0]);
                // lightKage[0] is the only value that's serialized
            }

            int lights = 1;

            if (strArray5 != null)
            {
                int numberOfLights = strArray5.Length - 1;
                lights += numberOfLights;

                for (int i = 0; i < numberOfLights; i++)
                {
                    string[] lightProperties = strArray5[i].Split(',');

                    int lightType = int.Parse(lightProperties[0]);

                    Color lightColor = new Color(
                        float.Parse(lightProperties[1]), float.Parse(lightProperties[1]),
                        float.Parse(lightProperties[1]), 1f
                    );

                    Quaternion lightAngle = Quaternion.Euler(
                        float.Parse(lightProperties[4]), float.Parse(lightProperties[5]), 18f
                    );

                    float intensity = float.Parse(lightProperties[6]);
                    float spotAngle = float.Parse(lightProperties[7]);
                    float range = spotAngle / 5f;
                    float shadowStrength = 0.098f;
                }
            }

            if (strArray7 != null)
            {
                for (int i = 0; i < lights; i++)
                {
                    string[] lightPosString = strArray7[i].Split(',');
                    Vector3 lightPosition = new Vector3(
                        float.Parse(lightPosString[0]), float.Parse(lightPosString[1]), float.Parse(lightPosString[2])
                    );
                }
            }

            // Message

            if (strArray3.Length > 16)
            {
                bool showingMessage = int.Parse(strArray3[34]) == 1;
                string name = strArray3[35];
                string message = strArray3[36].Replace("&kaigyo", "\n");
                // MM does not serialize message font size
            }

            // effect

            if (strArray4 != null)
            {
                // bloom
                bool bloomActive = int.Parse(strArray4[1]) == 1;
                float bloomIntensity = float.Parse(strArray4[2]);
                float bloomBlurIterations = float.Parse(strArray4[3]);
                Color bloomColour = new Color(
                    float.Parse(strArray4[4]), float.Parse(strArray4[5]), float.Parse(strArray4[6]), 1f
                );
                bool bloomHdr = int.Parse(strArray4[7]) == 1;

                // vignetting
                bool vignetteActive = int.Parse(strArray4[8]) == 1;
                float vignetteIntensity = float.Parse(strArray4[9]);
                float vignetteBlur = float.Parse(strArray4[10]);
                float vignetteBlurSpread = float.Parse(strArray4[11]);
                float vignetteChromaticAberration = float.Parse(strArray4[12]);

                // bokashi (TODO: implement in MPS)
                float bokashi = float.Parse(strArray4[13]);

                if (strArray4.Length > 15)
                {
                    bool dofActive = int.Parse(strArray4[15]) == 1;
                    float dofFocalLength = float.Parse(strArray4[16]);
                    float dofFocalSize = float.Parse(strArray4[17]);
                    float dofAperture = float.Parse(strArray4[18]);
                    float dofMaxBlurSize = float.Parse(strArray4[19]);
                    bool dofVisualizeFocus = int.Parse(strArray4[20]) == 1;

                    bool fogActive = int.Parse(strArray4[21]) == 1;
                    float fogStartDistance = float.Parse(strArray4[22]);
                    float fogDensity = float.Parse(strArray4[23]);
                    float fogHeightScale = float.Parse(strArray4[24]);
                    float fogHeight = float.Parse(strArray4[25]);
                    Color fogColor = new Color(
                        float.Parse(strArray4[26]), float.Parse(strArray4[27]), float.Parse(strArray4[28]), 1f
                    );
                }
            }

            // prop
            if (strArray3.Length > 37 && !string.IsNullOrEmpty(strArray3[37]))
            {
                // For the prop that spawns when you push (shift +) W
                string assetName = strArray3[37].Replace(' ', '_');
                Vector3 position = new Vector3(
                    float.Parse(strArray3[41]), float.Parse(strArray3[42]), float.Parse(strArray3[43])
                );
                Quaternion rotation = Quaternion.Euler(
                    float.Parse(strArray3[38]), float.Parse(strArray3[39]), float.Parse(strArray3[40])
                );
                Vector3 localScale = new Vector3(
                    float.Parse(strArray3[44]), float.Parse(strArray3[45]), float.Parse(strArray3[46])
                );
            }

            if (strArray6 != null)
            {
                for (int i = 0; i < strArray6.Length - 1; i++)
                {
                    string[] assetParts = strArray6[i].Split(',');
                    string assetName = assetParts[0].Replace(' ', '_');

                    if (assetName.StartsWith("creative_"))
                    {
                        // modifiedMM my room creative prop
                        // modifiedMM serializes the prefabName rather than the ID.
                        // TODO: Either write a special case for MPS or rewrite for use in game
                        assetName.Replace("creative_", String.Empty);
                        assetName = $"MYR_#{assetName}";
                    }
                    // else if (assetName.StartsWith("MYR_"))
                    // {
                    //     // MM 23.0+ my room creative prop
                    //     assetName = assetName + "#";
                    // }
                    else if (assetName.Contains('#'))
                    {
                        if (assetName.Contains(".menu"))
                        {
                            // modifiedMM official mod prop
                            string[] modComponents = assetParts[0].Split('#');
                            string baseMenuFile = modComponents[0].Replace(' ', '_');
                            string modItem = modComponents[1].Replace(' ', '_');
                            assetName = $"{modComponents[0]}#{modComponents[1]}";
                        }
                        else
                        {
                            assetName = assetName.Split('#')[1].Replace(' ', '_');
                        }
                    }

                    writer.WriteLine(assetName);

                    Vector3 position = new Vector3(
                        float.Parse(assetParts[4]), float.Parse(assetParts[5]), float.Parse(assetParts[6])
                    );
                    Quaternion rotation = Quaternion.Euler(
                        float.Parse(assetParts[1]), float.Parse(assetParts[2]), float.Parse(assetParts[3])
                    );
                    Vector3 scale = new Vector3(
                        float.Parse(assetParts[7]), float.Parse(assetParts[8]), float.Parse(assetParts[9])
                    );
                }
            }

            // meido

            int numberOfMaids = strArray2.Length;

            for (int i = 0; i < numberOfMaids; i++)
            {
                List<Quaternion> fingerRotation = new List<Quaternion>();
                string[] maidData = strArray2[i].Split(':');
                for (int j = 0; j < 40; j++)
                {
                    string fingerString = maidData[j];
                    fingerRotation.Add(Quaternion.FromEulerString(fingerString));
                }

                // TODO: Other maid related things
            }
        }
    }

    public static class BinaryExtensions
    {
        public static void WriteVector3(this BinaryWriter binaryWriter, Vector3 vector3)
        {
            binaryWriter.Write(vector3.x);
            binaryWriter.Write(vector3.y);
            binaryWriter.Write(vector3.z);
        }

        public static void WriteQuaternion(this BinaryWriter binaryWriter, Quaternion quaternion)
        {
            binaryWriter.Write(quaternion.x);
            binaryWriter.Write(quaternion.y);
            binaryWriter.Write(quaternion.z);
            binaryWriter.Write(quaternion.w);
        }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static Vector3 FromString(string vector3)
        {
            string[] data = vector3.Split(',');
            return new Vector3(
                float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])
            );
        }
    }

    public struct Quaternion
    {
        public const float DegToRad = MathF.PI / 180f;
        public float x, y, z, w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion Euler(float x, float y, float z)
        {
            System.Numerics.Quaternion q = System.Numerics.Quaternion.CreateFromYawPitchRoll(
                y * DegToRad, x * DegToRad, z * DegToRad
            );
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static Quaternion FromEulerString(string euler)
        {
            Vector3 components = Vector3.FromString(euler);
            return Euler(components.x, components.y, components.z);
        }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}
