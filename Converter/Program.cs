using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zlib;
using ExIni;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyRoomCustom;

namespace COM3D2.MeidoPhotoStudio.Converter
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class SceneConverter : BaseUnityPlugin
    {
        private const string pluginGuid = "com.habeebweeb.com3d2.meidophotostudio.converter";
        public const string pluginName = "MeidoPhotoStudio Converter";
        public const string pluginVersion = "0.0.0";
        private readonly byte[] noThumb = {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00,
            0x00, 0x32, 0x00, 0x00, 0x00, 0x32, 0x08, 0x02, 0x00, 0x00, 0x00, 0x91, 0x5D, 0x1F, 0xE6, 0x00, 0x00, 0x00,
            0x01, 0x73, 0x52, 0x47, 0x42, 0x00, 0xAE, 0xCE, 0x1C, 0xE9, 0x00, 0x00, 0x00, 0x04, 0x67, 0x41, 0x4D, 0x41,
            0x00, 0x00, 0xB1, 0x8F, 0x0B, 0xFC, 0x61, 0x05, 0x00, 0x00, 0x00, 0x09, 0x70, 0x48, 0x59, 0x73, 0x00, 0x00,
            0x0E, 0xC3, 0x00, 0x00, 0x0E, 0xC3, 0x01, 0xC7, 0x6F, 0xA8, 0x64, 0x00, 0x00, 0x01, 0x4E, 0x49, 0x44, 0x41,
            0x54, 0x58, 0x47, 0xDD, 0xD2, 0x5B, 0x8E, 0x83, 0x30, 0x10, 0x44, 0xD1, 0x2C, 0x84, 0x4F, 0xF6, 0xBF, 0xB3,
            0xAC, 0x21, 0xA9, 0xA8, 0x90, 0x01, 0x63, 0xEC, 0x7E, 0xD9, 0x46, 0xB9, 0xCA, 0xCF, 0x44, 0x72, 0xD7, 0x89,
            0x34, 0xAF, 0xF7, 0xB2, 0x3E, 0xF0, 0xB3, 0xB1, 0x3E, 0x8F, 0x69, 0x67, 0xF1, 0x0F, 0x7E, 0x3B, 0xB7, 0x84,
            0xD9, 0x58, 0xE9, 0xAB, 0x89, 0x1D, 0x25, 0x3B, 0x0B, 0x4D, 0x94, 0x65, 0x8C, 0x13, 0x0B, 0x4D, 0x91, 0x5D,
            0x0D, 0x39, 0x0B, 0x0D, 0x96, 0x15, 0x01, 0x05, 0x16, 0x1A, 0x26, 0xBB, 0x5B, 0x2F, 0xB3, 0xD0, 0x00, 0x59,
            0x65, 0xFA, 0x96, 0x85, 0xBA, 0xCA, 0xEA, 0xBB, 0x35, 0x16, 0xEA, 0x24, 0x6B, 0x8E, 0x36, 0x58, 0x28, 0x5C,
            0x26, 0x59, 0x6C, 0xB3, 0x50, 0xA0, 0x4C, 0x38, 0x27, 0x62, 0xA1, 0x10, 0x99, 0x7C, 0x4B, 0xCA, 0x42, 0x4E,
            0x99, 0x6A, 0x48, 0xC1, 0x42, 0x66, 0x99, 0x76, 0x45, 0xC7, 0x42, 0x06, 0x99, 0x61, 0x42, 0xCD, 0x42, 0xAA,
            0x27, 0xB6, 0xFB, 0x16, 0x16, 0x12, 0xBE, 0x32, 0x1F, 0x37, 0xB2, 0x50, 0xF3, 0xA1, 0xE7, 0xB2, 0x9D, 0x85,
            0x2A, 0x6F, 0x9D, 0x67, 0x5D, 0x2C, 0x54, 0x7C, 0xEE, 0xBF, 0xE9, 0x65, 0xA1, 0xEC, 0x42, 0xC8, 0xC1, 0x00,
            0x16, 0x4A, 0x47, 0xA2, 0xAE, 0xC5, 0xB0, 0x10, 0xEE, 0x04, 0x9E, 0xFA, 0x6B, 0x56, 0x3A, 0x12, 0x75, 0xED,
            0x4F, 0xFF, 0xE5, 0x8B, 0xCF, 0xFD, 0x37, 0x5D, 0xAC, 0xCA, 0x5B, 0xE7, 0x59, 0x3B, 0xAB, 0xF9, 0xD0, 0x73,
            0xD9, 0xC8, 0x12, 0xBE, 0x32, 0x1F, 0xB7, 0xB0, 0x54, 0x4F, 0x6C, 0xF7, 0xD5, 0xAC, 0xDE, 0x3F, 0x03, 0xA9,
            0x59, 0x06, 0x13, 0xD3, 0xAE, 0x28, 0x58, 0x66, 0x13, 0x53, 0x0D, 0x49, 0x59, 0x4E, 0x13, 0x93, 0x6F, 0x89,
            0x58, 0x21, 0x26, 0x26, 0x9C, 0x6B, 0xB3, 0x02, 0x4D, 0x4C, 0xB2, 0xD8, 0x60, 0x85, 0x9B, 0x58, 0x73, 0xB4,
            0xC6, 0xEA, 0x64, 0x62, 0xF5, 0xDD, 0x5B, 0x56, 0x57, 0x13, 0xAB, 0x4C, 0x97, 0x59, 0x03, 0x4C, 0xEC, 0x6E,
            0xBD, 0xC0, 0x1A, 0x66, 0x62, 0x45, 0x40, 0xCE, 0x1A, 0x6C, 0x62, 0x57, 0xC3, 0x89, 0x35, 0xC5, 0xC4, 0x32,
            0xC6, 0xCE, 0x9A, 0x68, 0x62, 0x47, 0xC9, 0xC6, 0x9A, 0x6E, 0x62, 0x09, 0xF3, 0x63, 0x3D, 0xC4, 0xC4, 0xE8,
            0xD9, 0x58, 0xCF, 0xFA, 0x2C, 0xEB, 0x17, 0xC3, 0x4F, 0x1F, 0x0A, 0xB7, 0x49, 0x8A, 0x9E, 0x00, 0x00, 0x00,
            0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        };
        private const string converterDirectoryName = "Converter";
        private static Dictionary<string, PlacementData.Data> myrAssetNameToData
            = new Dictionary<string, PlacementData.Data>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly string[] faceKeys = {
            "eyeclose", "eyeclose2", "eyeclose3", "eyeclose6", "hitomih", "hitomis", "mayuha",
            "mayuup", "mayuv", "mayuvhalf", "moutha", "mouths", "mouthdw", "mouthup", "tangout",
            "tangup", "eyebig", "eyeclose5", "mayuw", "mouthhe", "mouthc", "mouthi", "mouthuphalf",
            "tangopen",
            "namida", "tear1", "tear2", "tear3", "shock", "yodare", "hoho", "hoho2", "hohos", "hohol",
            "toothoff", "nosefook"
        };
        private static readonly int[] bodyRotations =
        {
            71, 44, 40, 41, 42, 43, 57, 68, 69, 46, 49, 47, 50, 52, 55, 53, 56, 45, 48, 51, 54
        };
        public const int sceneVersion = 1100;
        public const int kankyoMagic = -765;
        private static BepInEx.Logging.ManualLogSource Log;
        private static readonly int faceToggleIndex = Array.IndexOf(faceKeys, "tangopen") + 1;
        private static string configPath = Path.Combine(Paths.ConfigPath, converterDirectoryName);
        private bool active = false;
        private Rect windowRect = new Rect(30f, 30f, 300f, 200f);

        private void Awake()
        {
            DontDestroyOnLoad(this);

            if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
            Log = Logger;
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            List<PlacementData.Data> dataList = PlacementData.GetAllDatas(false);

            foreach (var data in dataList)
            {
                string assetName = string.IsNullOrEmpty(data.assetName) ? data.resourceName : data.assetName;
                myrAssetNameToData[assetName] = data;
            }
        }

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
            GUILayout.Space(30f);
            if (GUILayout.Button("Convert modifedMM quickSave")) ProcessQuickSave();
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

        private void ProcessQuickSave()
        {
            string sybarisPath = Path.Combine(Paths.GameRootPath, "Sybaris");
            string iniPath = BepInEx.Utility.CombinePaths(sybarisPath, "UnityInjector", "Config", "MultipleMaids.ini");

            IniFile mmIniFile = IniFile.FromFile(iniPath);

            IniSection sceneSection = mmIniFile.GetSection("scene");

            if (sceneSection != null)
            {
                if (sceneSection.HasKey("s9999"))
                {
                    string sceneData = sceneSection["s9999"].Value;

                    if (!string.IsNullOrEmpty(sceneData))
                    {
                        byte[] convertedSceneData = ProcessScene(sceneData, false);
                        string path = Path.Combine(configPath, $"mmtempscene{GetMMDateString(sceneData)}.png");
                        SaveSceneToFile(path, convertedSceneData, noThumb);
                    }
                }
            }
        }

        private void ProcessModifedMM()
        {
            string sybarisPath = Path.Combine(Paths.GameRootPath, "Sybaris");
            string iniPath = BepInEx.Utility.CombinePaths(sybarisPath, "UnityInjector", "Config", "MultipleMaids.ini");

            IniFile mmIniFile = IniFile.FromFile(iniPath);

            IniSection sceneSection = mmIniFile.GetSection("scene");

            if (sceneSection != null)
            {
                foreach (IniKey key in sceneSection.Keys)
                {
                    if (key.Key.StartsWith("ss")) continue;

                    bool kankyo = int.Parse(key.Key.Substring(1)) >= 10000;
                    string sceneData = key.Value;

                    if (!string.IsNullOrEmpty(sceneData))
                    {
                        byte[] convertedSceneData = ProcessScene(sceneData, kankyo);

                        string prefix = kankyo ? "mmkankyo" : "mmscene";

                        string path = Path.Combine(configPath, $"{prefix}_{key.Key}{GetMMDateString(sceneData)}.png");

                        byte[] thumbnail = noThumb;

                        string screenshotKey = $"s{key.Key}";
                        if (sceneSection.HasKey(screenshotKey))
                        {
                            string screenshotBase64 = sceneSection[screenshotKey].Value;
                            if (!string.IsNullOrEmpty(screenshotBase64))
                            {
                                thumbnail = Convert.FromBase64String(screenshotBase64);
                            }
                        }

                        SaveSceneToFile(path, convertedSceneData, thumbnail);
                    }
                }
            }
        }

        public static void SaveSceneToFile(string path, byte[] sceneData, byte[] thumbnailData)
        {
            using (FileStream fileStream = File.Create(path))
            {
                fileStream.Write(thumbnailData, 0, thumbnailData.Length);
                fileStream.Write(sceneData, 0, sceneData.Length);
            }
        }

        public static string GetMMDateString(string sceneData)
        {
            string dateString = sceneData.Split('_')[0].Split(',')[0];
            DateTime date = DateTime.Parse(dateString);
            return $"{date:yyyyMMddHHmm}";
        }

        public static byte[] ProcessScene(string sceneData, bool kankyo)
        {
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

            using (MemoryStream memoryStream = new MemoryStream())
            using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            using (BinaryWriter binaryWriter = new BinaryWriter(deflateStream, System.Text.Encoding.UTF8))
            {
                binaryWriter.Write("MPS_SCENE");
                binaryWriter.Write(sceneVersion);

                binaryWriter.Write(kankyo ? kankyoMagic : int.Parse(strArray3[1]));

                SerializeEnvironment(strArray3, binaryWriter, kankyo);

                SerializeLights(strArray3, strArray4, strArray5, strArray7, binaryWriter);

                SerializeMessage(strArray3, binaryWriter);

                SerializeEffect(strArray4, binaryWriter);

                SerializeProp(strArray3, strArray6, binaryWriter);

                SerializeMaid(strArray2, binaryWriter);

                binaryWriter.Write("END");

                deflateStream.Close();

                return memoryStream.ToArray();
            }
        }

        private static void SerializeMaid(string[] strArray2, BinaryWriter binaryWriter)
        {
            binaryWriter.Write("MEIDO");
            // MM scene converted to MPS
            binaryWriter.Write(true);

            int numberOfMaids = strArray2.Length;

            binaryWriter.Write(numberOfMaids);

            /*
                TODO: Investigate why serialized maid data may only have 64 items.
                https://git.coder.horse/meidomustard/modifiedMM/src/master/MultipleMaids/CM3D2/MultipleMaids/Plugin/MultipleMaids.Update.cs#L3745
                
                The difference affects whether or not rotations are local or world. 
                Certain body rotations would be missing as well particularly the toes.
                Other data like free look and attached items like hand/vag/anl would be missing.
            */

            for (int i = 0; i < numberOfMaids; i++)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                using (BinaryWriter tempWriter = new BinaryWriter(memoryStream))
                {
                    string[] maidData = strArray2[i].Split(':');
                    tempWriter.WriteVector3(Utility.Vector3String(maidData[59])); // position
                    tempWriter.WriteQuaternion(Utility.EulerString(maidData[58])); // rotation
                    tempWriter.WriteVector3(Utility.Vector3String(maidData[60])); // scale

                    // fingers
                    for (int j = 0; j < 40; j++)
                    {
                        tempWriter.WriteQuaternion(Utility.EulerString(maidData[j]));
                    }

                    // toes
                    for (int k = 0; k < 2; k++)
                    {
                        for (int j = 72 + k; j < 90; j += 2)
                        {
                            tempWriter.WriteQuaternion(Utility.EulerString(maidData[j]));
                        }
                    }

                    // the rest of the limbs
                    foreach (int j in bodyRotations)
                    {
                        tempWriter.WriteQuaternion(Utility.EulerString(maidData[j]));
                    }

                    tempWriter.WriteVector3(Utility.Vector3String(maidData[96])); // hip position

                    // cached pose stuff
                    tempWriter.Write("normal");
                    tempWriter.Write("pose_taiki_f");
                    tempWriter.Write(false);

                    // eye rotation delta
                    // MM saves the rotations directly so just save the identity
                    tempWriter.WriteQuaternion(Quaternion.identity);
                    tempWriter.WriteQuaternion(Quaternion.identity);

                    string[] freeLookData = maidData[64].Split(',');

                    tempWriter.Write(int.Parse(freeLookData[0]) == 1);
                    tempWriter.WriteVector3(new Vector3(
                        float.Parse(freeLookData[2]), 1f, float.Parse(freeLookData[1])
                    ));

                    string[] faceValues = maidData[63].Split(',');

                    tempWriter.Write("MPS_FACE");
                    for (int j = 0; j < faceKeys.Length - 2; j++)
                    {
                        tempWriter.Write(faceKeys[j]);
                        if (j >= faceToggleIndex) tempWriter.Write(float.Parse(faceValues[j]) > 0f);
                        else tempWriter.Write(float.Parse(faceValues[j]));
                    }

                    if (faceValues.Length > 65)
                    {
                        tempWriter.Write(faceKeys[faceKeys.Length - 1]);
                        tempWriter.Write(float.Parse(faceValues[faceValues.Length - 1]) > 0f);
                    }
                    tempWriter.Write("END_FACE");

                    tempWriter.Write(true); // body visible

                    // MM does not serialize clothing
                    for (int j = 0; j < 29; j++) tempWriter.Write(true);

                    // MM does not serialize curling
                    tempWriter.Write(false);
                    tempWriter.Write(false);
                    tempWriter.Write(false);

                    binaryWriter.Write(memoryStream.Length);
                    binaryWriter.Write(memoryStream.ToArray());
                }
            }
        }

        private static void SerializeProp(string[] strArray3, string[] strArray6, BinaryWriter binaryWriter)
        {
            binaryWriter.Write("PROP");

            bool hasWProp = strArray3.Length > 37 && !string.IsNullOrEmpty(strArray3[37]);
            int numberOfProps = hasWProp ? 1 : 0;
            numberOfProps += strArray6 == null ? 0 : strArray6.Length - 1;

            binaryWriter.Write(numberOfProps);

            if (hasWProp)
            {
                // For the prop that spawns when you push (shift +) W
                binaryWriter.Write(strArray3[37].Replace(' ', '_'));

                SerializeAttachPoint(binaryWriter);

                binaryWriter.WriteVector3(new Vector3(
                    float.Parse(strArray3[41]), float.Parse(strArray3[42]), float.Parse(strArray3[43])
                ));

                binaryWriter.WriteQuaternion(Quaternion.Euler(
                    float.Parse(strArray3[38]), float.Parse(strArray3[39]), float.Parse(strArray3[40])
                ));

                binaryWriter.WriteVector3(new Vector3(
                    float.Parse(strArray3[44]), float.Parse(strArray3[45]), float.Parse(strArray3[46])
                ));
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
                        assetName = assetName.Replace("creative_", String.Empty);
                        assetName = $"MYR_{myrAssetNameToData[assetName].ID}#{assetName}";
                    }
                    else if (assetName.StartsWith("MYR_"))
                    {
                        // MM 23.0+ my room creative prop
                        PlacementData.Data data = myrAssetNameToData[assetName];
                        string asset = string.IsNullOrEmpty(data.assetName) ? data.resourceName : data.assetName;

                        assetName = $"{assetName}#{asset}";
                    }
                    else if (assetName.Contains('#'))
                    {
                        if (assetName.Contains(".menu"))
                        {
                            // modifiedMM official mod prop
                            string[] modComponents = assetParts[0].Split('#');
                            string baseMenuFile = modComponents[0].Replace(' ', '_');
                            string modItem = modComponents[1].Replace(' ', '_');
                            assetName = $"{modItem}#{baseMenuFile}";
                        }
                        else
                        {
                            assetName = assetName.Split('#')[1].Replace(' ', '_');
                        }
                    }

                    binaryWriter.Write(assetName);

                    SerializeAttachPoint(binaryWriter);

                    binaryWriter.WriteVector3(new Vector3(
                        float.Parse(assetParts[4]), float.Parse(assetParts[5]), float.Parse(assetParts[6])
                    ));

                    binaryWriter.WriteQuaternion(Quaternion.Euler(
                        float.Parse(assetParts[1]), float.Parse(assetParts[2]), float.Parse(assetParts[3])
                    ));

                    binaryWriter.WriteVector3(new Vector3(
                        float.Parse(assetParts[7]), float.Parse(assetParts[8]), float.Parse(assetParts[9])
                    ));
                }
            }
        }

        private static void SerializeEffect(string[] strArray4, BinaryWriter binaryWriter)
        {
            binaryWriter.Write("EFFECT");

            if (strArray4 != null)
            {
                // bloom
                binaryWriter.Write("EFFECT_BLOOM");
                binaryWriter.Write(float.Parse(strArray4[2])); // intensity
                binaryWriter.Write((int)float.Parse(strArray4[3])); // blur iterations
                binaryWriter.WriteColour(new Color( // bloom threshold colour
                    float.Parse(strArray4[4]), float.Parse(strArray4[5]), float.Parse(strArray4[6]), 1f
                ));
                binaryWriter.Write(int.Parse(strArray4[7]) == 1); // hdr
                binaryWriter.Write(int.Parse(strArray4[1]) == 1); // active

                // vignetting
                binaryWriter.Write("EFFECT_VIGNETTE");
                binaryWriter.Write(float.Parse(strArray4[9])); // intensity
                binaryWriter.Write(float.Parse(strArray4[10])); // blur
                binaryWriter.Write(float.Parse(strArray4[11])); // blur spread
                binaryWriter.Write(float.Parse(strArray4[12])); // chromatic aberration
                binaryWriter.Write(int.Parse(strArray4[8]) == 1); // active

                // bokashi 
                // TODO: implement bokashi in MPS
                float bokashi = float.Parse(strArray4[13]);

                // TODO: implement sepia in MPS too

                if (strArray4.Length > 15)
                {
                    binaryWriter.Write("EFFECT_DOF");
                    binaryWriter.Write(float.Parse(strArray4[16])); // focal length
                    binaryWriter.Write(float.Parse(strArray4[17])); // focal size
                    binaryWriter.Write(float.Parse(strArray4[18])); // aperture
                    binaryWriter.Write(float.Parse(strArray4[19])); // max blur size
                    binaryWriter.Write(int.Parse(strArray4[20]) == 1); // visualize focus
                    binaryWriter.Write(int.Parse(strArray4[15]) == 1); // active

                    binaryWriter.Write("EFFECT_FOG");
                    binaryWriter.Write(float.Parse(strArray4[22])); // fog distance
                    binaryWriter.Write(float.Parse(strArray4[23])); // density
                    binaryWriter.Write(float.Parse(strArray4[24])); // height scale
                    binaryWriter.Write(float.Parse(strArray4[25])); // height
                    binaryWriter.WriteColour(new Color( // fog colour
                        float.Parse(strArray4[26]), float.Parse(strArray4[27]), float.Parse(strArray4[28]), 1f
                    ));
                    binaryWriter.Write(int.Parse(strArray4[21]) == 1); // active
                }
            }

            binaryWriter.Write("END_EFFECT");
        }

        private static void SerializeMessage(string[] strArray3, BinaryWriter binaryWriter)
        {
            binaryWriter.Write("TEXTBOX");

            bool showingMessage = false;
            string name = "Maid";
            string message = "Hello world";

            if (strArray3.Length > 16)
            {
                showingMessage = int.Parse(strArray3[34]) == 1;
                name = strArray3[35];
                message = strArray3[36].Replace("&kaigyo", "\n");
                // MM does not serialize message font size
            }

            binaryWriter.Write(showingMessage);
            binaryWriter.Write(25);
            binaryWriter.WriteNullableString(name);
            binaryWriter.WriteNullableString(message);
        }

        private static void SerializeLights(string[] strArray3, string[] strArray4, string[] strArray5, string[] strArray7, BinaryWriter binaryWriter)
        {
            // Lights
            binaryWriter.Write("LIGHT");

            int numberOfLights = 1;
            numberOfLights += strArray5 == null ? 0 : strArray5.Length - 1;

            binaryWriter.Write(numberOfLights);

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
                Color lightColour = new Color(
                    float.Parse(strArray3[18]), float.Parse(strArray3[19]), float.Parse(strArray3[20]), 1f
                );

                Quaternion lightRotation = Quaternion.Euler(
                    float.Parse(strArray3[21]), float.Parse(strArray3[22]), float.Parse(strArray3[23])
                );

                // MM uses spotAngle for both range and spotAngle based on which light type is used
                float intensity = float.Parse(strArray3[24]);
                float spotAngle = float.Parse(strArray3[25]);
                float range = lightType == 2 ? spotAngle / 5f : spotAngle; ;
                float shadowStrength = 0.098f;
                if (strArray4 != null) shadowStrength = float.Parse(strArray4[0]);

                for (int i = 0; i < 3; i++)
                {
                    if (i == lightType || (i == 0 && lightType == 3))
                    {
                        SerializeLightProperty(
                            binaryWriter, lightRotation, lightColour, intensity, spotAngle, range, shadowStrength
                        );
                    }
                    else SerializeDefaultLight(binaryWriter);
                }

                if (strArray7 != null)
                {
                    binaryWriter.WriteVector3(Utility.Vector3String(strArray7[0]));
                }
                else binaryWriter.WriteVector3(new Vector3(0f, 1.9f, 0.4f));
                binaryWriter.Write(lightType == 3 ? 0 : lightType);
                binaryWriter.Write(lightType == 3);
                binaryWriter.Write(false);
                // lightKage[0] is the only value that's serialized
            }


            if (strArray5 != null)
            {
                int otherLights = strArray5.Length - 1;
                for (int i = 0; i < otherLights; i++)
                {
                    string[] lightProperties = strArray5[i].Split(',');

                    int lightType = int.Parse(lightProperties[0]);

                    Color lightColour = new Color(
                        float.Parse(lightProperties[1]), float.Parse(lightProperties[2]),
                        float.Parse(lightProperties[3]), 1f
                    );

                    Quaternion lightRotation = Quaternion.Euler(
                        float.Parse(lightProperties[4]), float.Parse(lightProperties[5]), 18f
                    );

                    float intensity = float.Parse(lightProperties[6]);
                    float spotAngle = float.Parse(lightProperties[7]);
                    float range = lightType == 2 ? spotAngle / 5f : spotAngle;
                    float shadowStrength = 0.098f;
                    for (int j = 0; j < 3; j++)
                    {
                        if (j == lightType)
                        {
                            SerializeLightProperty(
                                binaryWriter, lightRotation, lightColour, intensity, spotAngle, range, shadowStrength
                            );
                        }
                        else SerializeDefaultLight(binaryWriter);
                    }
                    if (strArray7 != null)
                    {
                        binaryWriter.WriteVector3(Utility.Vector3String(strArray7[i + 1]));
                    }
                    else binaryWriter.WriteVector3(new Vector3(0f, 1.9f, 0.4f));
                    binaryWriter.Write(lightType == 3 ? 0 : lightType);
                    binaryWriter.Write(false);
                    binaryWriter.Write(lightType == 3);
                }
            }
        }

        private static void SerializeEnvironment(string[] data, BinaryWriter binaryWriter, bool kankyo)
        {
            binaryWriter.Write("ENVIRONMENT");

            int bgIndex;

            string bgAsset = "Theater";

            if (!int.TryParse(data[2], out bgIndex))
            {
                bgAsset = data[2].Replace(" ", "_");
            }

            binaryWriter.Write(bgAsset);

            binaryWriter.WriteVector3(new Vector3(
                float.Parse(data[6]), float.Parse(data[7]), float.Parse(data[8])
            ));

            binaryWriter.WriteQuaternion(Quaternion.Euler(
                float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5])
            ));

            binaryWriter.WriteVector3(new Vector3(
                float.Parse(data[9]), float.Parse(data[10]), float.Parse(data[11])
            ));

            binaryWriter.Write(kankyo);

            Vector3 cameraTargetPos = new Vector3(0f, 0.9f, 0f);
            float cameraDistance = 3f;
            Quaternion cameraRotation = Quaternion.identity;

            if (data.Length > 16)
            {
                cameraTargetPos = new Vector3(
                    float.Parse(data[27]), float.Parse(data[28]), float.Parse(data[29])
                );

                cameraDistance = float.Parse(data[30]);

                cameraRotation = Quaternion.Euler(
                    float.Parse(data[31]), float.Parse(data[32]), float.Parse(data[33])
                );
            }

            binaryWriter.WriteVector3(cameraTargetPos);

            binaryWriter.Write(cameraDistance);

            binaryWriter.WriteQuaternion(cameraRotation);
        }

        public static void SerializeAttachPoint(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(0);
            binaryWriter.Write(-1);
        }

        public static void SerializeDefaultLight(BinaryWriter binaryWriter)
        {
            SerializeLightProperty(binaryWriter, Quaternion.Euler(40f, 180f, 0f), Color.white);
        }

        public static void SerializeLightProperty(
            BinaryWriter binaryWriter,
            Quaternion rotation, Color colour, float intensity = 0.95f, float range = 50f,
            float spotAngle = 50f, float shadowStrength = 0.1f
        )
        {
            binaryWriter.WriteQuaternion(rotation);
            binaryWriter.Write(intensity);
            binaryWriter.Write(range);
            binaryWriter.Write(spotAngle);
            binaryWriter.Write(shadowStrength);
            binaryWriter.WriteColour(colour);
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

        public static void WriteColour(this BinaryWriter binaryWriter, UnityEngine.Color colour)
        {
            binaryWriter.Write(colour.r);
            binaryWriter.Write(colour.g);
            binaryWriter.Write(colour.b);
            binaryWriter.Write(colour.a);
        }

        public static void WriteNullableString(this BinaryWriter binaryWriter, string str)
        {
            binaryWriter.Write(str != null);
            if (str != null) binaryWriter.Write(str);
        }
    }


    public static class Utility
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
