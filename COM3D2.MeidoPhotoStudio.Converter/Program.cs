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
    using static Plugin.BinaryExtensions;

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class SceneConverter : BaseUnityPlugin
    {
        private const string pluginGuid = "com.habeebweeb.com3d2.meidophotostudio.converter";
        public const string pluginName = "MeidoPhotoStudio Converter";
        public const string pluginVersion = "0.0.0";
        private readonly byte[] noThumb = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAIAAACRXR/mAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7D" +
            "AcdvqGQAAAFOSURBVFhH3dJbjoMwEETRLIRP9r+zrCGpqJABY+x+2Ua5ys9EcteJNK/3sj7ws7E+j2ln8Q9+O7eE2Vjpq4kdJTsLTZRl" +
            "jBMLTZFdDTkLDZYVAQUWGia7Wy+z0ABZZfqWhbrK6rs1Fuoka442WChcJllss1CgTDgnYqEQmXxLykJOmWpIwUJmmXZFx0IGmWFCzUKq" +
            "J7b7FhYSvjIfN7JQ86Hnsp2FKm+dZ10sVHzuv+lloexCyMEAFkpHoq7FsBDuBJ76a1Y6EnXtT//li8/9N12sylvnWTur+dBz2cgSvjIf" +
            "t7BUT2z31azePwOpWQYT064oWGYTUw1JWU4Tk2+JWCEmJpxrswJNTLLYYIWbWHO0xupkYvXdW1ZXE6tMl1kDTOxuvcAaZmJFQM4abGJX" +
            "w4k1xcQyxs6aaGJHycaabmIJ82M9xMTo2VjP+izrF8NPHwq3SYqeAAAAAElFTkSuQmCC"
        );
        private static readonly Dictionary<string, PlacementData.Data> myrAssetNameToData
            = new Dictionary<string, PlacementData.Data>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly string[] faceKeys = {
            "eyeclose", "eyeclose2", "eyeclose3", "eyeclose6", "hitomih", "hitomis", "mayuha",
            "mayuup", "mayuv", "mayuvhalf", "moutha", "mouths", "mouthdw", "mouthup", "tangout",
            "tangup", "eyebig", "eyeclose5", "mayuw", "mouthhe", "mouthc", "mouthi", "mouthuphalf",
            "tangopen",
            "namida", "tear1", "tear2", "tear3", "shock", "yodare", "hoho", "hoho2", "hohos", "hohol",
            "toothoff", "nosefook"
        };
        private static readonly string[] mpnAttachProps = {
            /* "", "", "", "", "", "", "", "", "", */
            "kousokuu_tekaseone_i_.menu", "kousokuu_tekasetwo_i_.menu", "kousokul_ashikaseup_i_.menu",
            "kousokuu_tekasetwo_i_.menu", "kousokul_ashikasedown_i_.menu", "kousokuu_tekasetwodown_i_.menu",
            "kousokuu_ushirode_i_.menu", "kousokuu_smroom_haritsuke_i_.menu"
        };
        private static readonly int[] bodyRotations =
        {
            71, 44, 40, 41, 42, 43, 57, 68, 69, 46, 49, 47, 50, 52, 55, 53, 56, 92, 94, 93, 95, 45, 48, 51, 54
        };
        private static BepInEx.Logging.ManualLogSource Log;
        private static readonly string scenesPath = Plugin.Constants.scenesPath;
        private static readonly Vector3 DefaultSoftG = new Vector3(0f, -3f / 1000f, 0f);
        private bool active;
        private Rect windowRect = new Rect(30f, 30f, 300f, 200f);

        private void Awake()
        {
            DontDestroyOnLoad(this);

            if (!Directory.Exists(scenesPath)) Directory.CreateDirectory(scenesPath);
            Log = Logger;
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            foreach (var data in PlacementData.GetAllDatas(false))
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
                windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
                windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);
                windowRect = GUI.Window(0xEA4040, windowRect, GUIFunc, "MeidoPhotoStudio Converter");
            }
        }

        private void GUIFunc(int id)
        {
            if (GUILayout.Button("Convert ModifiedMM")) ProcessModifedMM();
            if (GUILayout.Button("Convert ModifiedMM (Scene Manager)")) ProcessModifiedMMPng();
            GUILayout.Space(30f);
            if (GUILayout.Button("Convert modifedMM quickSave")) ProcessQuickSave();
        }

        private void ProcessModifiedMMPng()
        {
            string modPath = Path.Combine(Paths.GameRootPath, "Mod");
            string scenePath = Path.Combine(modPath, "MultipleMaidsScene");
            string kankyoPath = Path.Combine(modPath, "MultipleMaidsScene");
        }

        private string GetModifiedMMSceneData(string pngPath)
        {
            return string.Empty;
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
                        string path = Path.Combine(scenesPath, $"mmtempscene_{GetMMDateString(sceneData)}.png");
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
                foreach (IniKey iniKey in sceneSection.Keys)
                {
                    if (iniKey.Key.StartsWith("ss")) continue;

                    int sceneIndex = int.Parse(iniKey.Key.Substring(1));
                    bool kankyo = sceneIndex >= 10000;
                    string sceneData = iniKey.Value;

                    if (!string.IsNullOrEmpty(sceneData))
                    {
                        byte[] convertedSceneData = ProcessScene(sceneData, kankyo);

                        string prefix = kankyo
                            ? "mmkankyo"
                            : sceneIndex == 9999
                                ? "mmtempscene" : $"mmscene{sceneIndex}";

                        string path = Path.Combine(scenesPath, $"{prefix}_{GetMMDateString(sceneData)}.png");

                        byte[] thumbnail = noThumb;

                        string screenshotKey = $"s{iniKey.Key}";
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
                binaryWriter.Write(Plugin.MeidoPhotoStudio.sceneVersion);

                binaryWriter.Write(kankyo ? Plugin.MeidoPhotoStudio.kankyoMagic : int.Parse(strArray3[1]));

                SerializeEnvironment(strArray3, binaryWriter, kankyo);
                SerializeLights(strArray3, strArray4, strArray5, strArray7, binaryWriter);
                SerializeEffect(strArray4, binaryWriter);
                SerializeProp(strArray3, strArray6, binaryWriter);

                if (!kankyo)
                {
                    SerializeMessage(strArray3, binaryWriter);
                    SerializeMaid(strArray2, strArray3, binaryWriter);
                }

                binaryWriter.Write("END");

                deflateStream.Close();

                return memoryStream.ToArray();
            }
        }

        private static void SerializeMaid(string[] strArray2, string[] strArray3, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Plugin.MeidoManager.header);
            // MM scene converted to MPS
            binaryWriter.Write(true);

            binaryWriter.Write(Plugin.Meido.meidoDataVersion);

            int numberOfMaids = strArray2.Length;

            binaryWriter.Write(numberOfMaids);

            /*
                TODO: Investigate why serialized maid data may only have 64 items.
                https://git.coder.horse/meidomustard/modifiedMM/src/master/MultipleMaids/CM3D2/MultipleMaids/Plugin/MultipleMaids.Update.cs#L3745
                
                The difference affects whether or not rotations are local or world. 
                Certain body rotations would be missing as well particularly the toes.
                Other data like free look and attached items like hand/vag/anl would be missing.
            */

            bool gravityEnabled = false;

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
                    tempWriter.Write("maid_stand01");
                    tempWriter.Write(false);

                    // eye rotation delta
                    // MM saves the rotations directly so just save the identity
                    tempWriter.WriteQuaternion(Quaternion.identity);
                    tempWriter.WriteQuaternion(Quaternion.identity);

                    string[] freeLookData = maidData[64].Split(',');

                    bool isFreeLook = int.Parse(freeLookData[0]) == 1;
                    tempWriter.Write(isFreeLook);
                    if (isFreeLook)
                    {
                        tempWriter.WriteVector3(new Vector3(
                            float.Parse(freeLookData[2]), 1f, float.Parse(freeLookData[1])
                        ));
                    }

                    // head/eye to camera
                    // MM does not changes these values so they're always true
                    tempWriter.Write(true);
                    tempWriter.Write(true);

                    string[] faceValues = maidData[63].Split(',');

                    tempWriter.Write("MPS_FACE");
                    for (int j = 0; j < faceKeys.Length - 2; j++)
                    {
                        tempWriter.Write(faceKeys[j]);
                        tempWriter.Write(float.Parse(faceValues[j]));
                    }

                    if (faceValues.Length > 65)
                    {
                        tempWriter.Write(faceKeys[faceKeys.Length - 1]);
                        tempWriter.Write(float.Parse(faceValues[faceValues.Length - 1]));
                    }
                    tempWriter.Write("END_FACE");

                    tempWriter.Write(true); // body visible

                    // MM does not serialize clothing
                    for (int j = 0; j < 29; j++) tempWriter.Write(true);

                    Vector3 softG = new Vector3(
                        float.Parse(strArray3[12]), float.Parse(strArray3[13]), float.Parse(strArray3[14])
                    );

                    bool hairGravityActive = softG != DefaultSoftG;
                    tempWriter.Write(hairGravityActive);
                    if (hairGravityActive)
                    {
                        // MM gravity affects all maids
                        gravityEnabled = true;
                        tempWriter.WriteVector3(softG * 5f);
                    }

                    // MM doesn't serialize skirt gravity
                    tempWriter.Write(false);

                    // MM does not serialize curling
                    tempWriter.Write(false);
                    tempWriter.Write(false);
                    tempWriter.Write(false);

                    string kousokuUpperMenu = string.Empty;
                    string kousokuLowerMenu = string.Empty;

                    int mpnIndex = int.Parse(maidData[65].Split(',')[0]);

                    // MM can attach accvag, accanl and handitem stuff as well as kousoku_upper/lower
                    // MPS attach prop is preferred for non kousoku_upper/lower props because unlike kousoku_upper/lower
                    // props, accvag etc. props attach only to a single place.
                    if (mpnIndex >= 9 && mpnIndex <= 16)
                    {
                        int actualIndex = mpnIndex - 9;
                        if (mpnIndex == 12)
                        {
                            kousokuUpperMenu = mpnAttachProps[actualIndex];
                            kousokuLowerMenu = mpnAttachProps[actualIndex - 1];
                        }
                        else if (mpnIndex == 13)
                        {
                            kousokuUpperMenu = mpnAttachProps[actualIndex + 1];
                            kousokuLowerMenu = mpnAttachProps[actualIndex];
                        }
                        else
                        {
                            if (mpnIndex > 13) actualIndex++;
                            string kousokuMenu = mpnAttachProps[actualIndex];
                            if (mpnAttachProps[actualIndex][7] == 'u') kousokuUpperMenu = kousokuMenu;
                            else kousokuLowerMenu = kousokuMenu;
                        }
                    }

                    bool kousokuUpper = !string.IsNullOrEmpty(kousokuUpperMenu);
                    tempWriter.Write(kousokuUpper);
                    if (kousokuUpper) tempWriter.Write(kousokuUpperMenu);

                    bool kousokuLower = !string.IsNullOrEmpty(kousokuLowerMenu);
                    tempWriter.Write(kousokuLower);
                    if (kousokuLower) tempWriter.Write(kousokuLowerMenu);

                    binaryWriter.Write(memoryStream.Length);
                    binaryWriter.Write(memoryStream.ToArray());
                }
            }

            binaryWriter.Write(gravityEnabled);
        }

        private static void SerializeProp(string[] strArray3, string[] strArray6, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Plugin.PropManager.header);

            binaryWriter.Write(Plugin.PropManager.propDataVersion);

            bool hasWProp = strArray3.Length > 37 && !string.IsNullOrEmpty(strArray3[37]);
            int numberOfProps = hasWProp ? 1 : 0;
            numberOfProps += strArray6 == null ? 0 : strArray6.Length - 1;

            binaryWriter.Write(numberOfProps);

            if (hasWProp)
            {
                // For the prop that spawns when you push (shift +) W

                binaryWriter.WriteVector3(new Vector3(
                    float.Parse(strArray3[41]), float.Parse(strArray3[42]), float.Parse(strArray3[43])
                ));

                binaryWriter.WriteQuaternion(Quaternion.Euler(
                    float.Parse(strArray3[38]), float.Parse(strArray3[39]), float.Parse(strArray3[40])
                ));

                binaryWriter.WriteVector3(new Vector3(
                    float.Parse(strArray3[44]), float.Parse(strArray3[45]), float.Parse(strArray3[46])
                ));

                SerializeAttachPoint(binaryWriter);

                binaryWriter.Write(false); // shadow casting

                binaryWriter.Write(strArray3[37].Replace(' ', '_'));
            }

            if (strArray6 != null)
            {
                for (int i = 0; i < strArray6.Length - 1; i++)
                {
                    string[] assetParts = strArray6[i].Split(',');
                    string assetName = assetParts[0].Replace(' ', '_');
                    bool shadowCasting = assetName.EndsWith(".menu");

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
                        int assetID = int.Parse(assetName.Replace("MYR_", string.Empty));
                        PlacementData.Data data = PlacementData.GetData(assetID);
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
                    else if (assetName.StartsWith("BGOdogu", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // I don't know why multiplemaids even prepends BG
                        assetName = assetName.Substring(2);
                    }

                    binaryWriter.WriteVector3(new Vector3(
                        float.Parse(assetParts[4]), float.Parse(assetParts[5]), float.Parse(assetParts[6])
                    ));

                    binaryWriter.WriteQuaternion(Quaternion.Euler(
                        float.Parse(assetParts[1]), float.Parse(assetParts[2]), float.Parse(assetParts[3])
                    ));

                    binaryWriter.WriteVector3(new Vector3(
                        float.Parse(assetParts[7]), float.Parse(assetParts[8]), float.Parse(assetParts[9])
                    ));

                    SerializeAttachPoint(binaryWriter);

                    binaryWriter.Write(shadowCasting);

                    binaryWriter.Write(assetName);
                }
            }
        }

        private static void SerializeEffect(string[] strArray4, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Plugin.EffectManager.header);

            if (strArray4 != null)
            {
                // bloom
                binaryWriter.Write(Plugin.BloomEffectManager.header);
                binaryWriter.Write(float.Parse(strArray4[2])); // intensity
                binaryWriter.Write((int)float.Parse(strArray4[3])); // blur iterations
                binaryWriter.WriteColour(new Color( // bloom threshold colour
                    float.Parse(strArray4[4]), float.Parse(strArray4[5]), float.Parse(strArray4[6]), 1f
                ));
                binaryWriter.Write(int.Parse(strArray4[7]) == 1); // hdr
                binaryWriter.Write(int.Parse(strArray4[1]) == 1); // active

                // vignetting
                binaryWriter.Write(Plugin.VignetteEffectManager.header);
                binaryWriter.Write(float.Parse(strArray4[9])); // intensity
                binaryWriter.Write(float.Parse(strArray4[10])); // blur
                binaryWriter.Write(float.Parse(strArray4[11])); // blur spread
                binaryWriter.Write(float.Parse(strArray4[12])); // chromatic aberration
                binaryWriter.Write(int.Parse(strArray4[8]) == 1); // active

                // bokashi 
                binaryWriter.Write(Plugin.BlurEffectManager.header);
                float blurSize = float.Parse(strArray4[13]);
                binaryWriter.Write(blurSize);
                binaryWriter.Write(blurSize > 0f);

                binaryWriter.Write(Plugin.SepiaToneEffectManger.header);
                binaryWriter.Write(int.Parse(strArray4[29]) == 1);

                if (strArray4.Length > 15)
                {
                    binaryWriter.Write(Plugin.DepthOfFieldEffectManager.header);
                    binaryWriter.Write(float.Parse(strArray4[16])); // focal length
                    binaryWriter.Write(float.Parse(strArray4[17])); // focal size
                    binaryWriter.Write(float.Parse(strArray4[18])); // aperture
                    binaryWriter.Write(float.Parse(strArray4[19])); // max blur size
                    binaryWriter.Write(int.Parse(strArray4[20]) == 1); // visualize focus
                    binaryWriter.Write(int.Parse(strArray4[15]) == 1); // active

                    binaryWriter.Write(Plugin.FogEffectManager.header);
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

            binaryWriter.Write(Plugin.EffectManager.footer);
        }

        private static void SerializeMessage(string[] strArray3, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Plugin.MessageWindowManager.header);

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
            binaryWriter.Write(Plugin.LightManager.header);

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
            binaryWriter.Write(Plugin.EnvironmentManager.header);

            string bgAsset = "Theater";

            if (!int.TryParse(data[2], out _))
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
