using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zlib;
using ExIni;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class SceneConverter : BaseUnityPlugin
    {
        private const string pluginGuid = "com.habeebweeb.com3d2.meidophotostudio.converter";
        public const string pluginName = "MeidoPhotoStudio Converter";
        public const string pluginVersion = "0.0.0";
        private const string converterDirectoryName = "Converter";
        private static string configPath = Path.Combine(Paths.ConfigPath, converterDirectoryName);
        private bool active = false;
        private Rect windowRect = new Rect(30f, 30f, 300f, 200f);

        private void Awake()
        {
            DontDestroyOnLoad(this);

            if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
        }

        private void Start() => UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            int index = scene.buildIndex;
            active = index == 9 || index == 3;
        }

        private void OnGUI()
        {
            if (active)
            {
                windowRect.width = 300f;
                windowRect.height = 200f;
                windowRect.x = UnityEngine.Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
                windowRect.y = UnityEngine.Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);
                windowRect = GUI.Window(0xEA4040, windowRect, GUIFunc, "MeidoPhotoStudio Converter");
            }
        }

        private void GUIFunc(int id)
        {
            if (GUILayout.Button("Convert ModifiedMM")) ProcessModifedMM();
            if (GUILayout.Button("Convert ModifiedMM (Scene Manager)")) ProcessModifiedMMPng();
            // GUILayout.Button("Convert MultipleMaids");
        }

        private void ProcessModifiedMMPng()
        {
            string modPath = Path.Combine(Paths.GameRootPath, "Mod");
            string scenePath = Path.Combine(modPath, "MultipleMaidsScene");
            string kankyoPath = Path.Combine(modPath, "MultipleMaidsScene");
        }

        private string GetModifiedMMSceneData(string pngPath)
        {
            return String.Empty;
        }

        private void ProcessModifedMM()
        {
            string sybarisPath = Path.Combine(Paths.GameRootPath, "Sybaris");
            string iniPath = Utility.CombinePaths(sybarisPath, "UnityInjector", "Config", "MultipleMaids.ini");

            IniFile mmIniFile = IniFile.FromFile(iniPath);

            IniSection sceneSection = mmIniFile.GetSection("scene");

            if (sceneSection != null)
            {
                foreach (IniKey key in sceneSection.Keys)
                {
                    if (key.Key.StartsWith("ss")) continue;
                    string sceneData = key.Value;
                    ProcessScene(sceneData);
                }
            }
        }

        public static void ProcessScene(string sceneData)
        {

            if (string.IsNullOrEmpty(sceneData)) return;

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

                // bokashi 
                // TODO: implement bokashi in MPS
                float bokashi = float.Parse(strArray4[13]);

                // TODO: implement sepia in MPS too

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
                    fingerRotation.Add(UnityStructExtensions.EulerString(fingerString));
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

    public static class UnityStructExtensions
    {
        public static Quaternion EulerString(string euler)
        {
            string[] data = euler.Split(',');
            return Quaternion.Euler(
                float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])
            );
        }

        public static Vector3 Vector3String(string vector3)
        {
            string[] data = vector3.Split(',');
            return new Vector3(
                float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])
            );
        }
    }
}
