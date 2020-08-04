using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class MenuFileUtility
    {
        public const string noCategory = "noCategory";
        public static string[] MenuCategories = new[] {
            noCategory, "acchat", "headset", "wear", "skirt", "onepiece", "mizugi", "bra", "panz", "stkg", "shoes",
            "acckami", "megane", "acchead", "acchana", "accmimi", "glove", "acckubi", "acckubiwa", "acckamisub",
            "accnip", "accude", "accheso", "accashi", "accsenaka", "accshippo", "accxxx"
        };
        private static readonly HashSet<string> accMpn = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private enum IMode
        {
            None, ItemChange, TexChange
        }
        public static event EventHandler MenuFilesReadyChange;
        public static bool MenuFilesReady { get; private set; } = false;

        static MenuFileUtility()
        {
            accMpn.UnionWith(MenuCategories.Skip(1));
            GameMain.Instance.StartCoroutine(CheckMenuDataBaseJob());
        }

        private static IEnumerator CheckMenuDataBaseJob()
        {
            if (MenuFilesReady) yield break;
            while (!GameMain.Instance.MenuDataBase.JobFinished()) yield return null;
            MenuFilesReady = true;
            MenuFilesReadyChange?.Invoke(null, EventArgs.Empty);
            yield break;
        }

        private static bool ProcScriptBin(byte[] menuBuf, ModelInfo modelInfo)
        {
            using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(menuBuf), Encoding.UTF8))
            {
                string menuHeader = binaryReader.ReadString();
                NDebug.Assert(
                    menuHeader == "CM3D2_MENU", "ProcScriptBin 例外 : ヘッダーファイルが不正です。" + menuHeader
                );
                binaryReader.ReadInt32(); // file version
                binaryReader.ReadString(); // txt path
                binaryReader.ReadString(); // name
                binaryReader.ReadString(); // category
                binaryReader.ReadString(); // description
                binaryReader.ReadInt32(); // idk (as long)

                string menuPropString = String.Empty;
                string menuPropStringTemp = String.Empty;
                // string tempString3 = String.Empty;
                // string slotName = String.Empty;

                try
                {
                    while (true)
                    {
                        int numberOfProps = (int)binaryReader.ReadByte();
                        menuPropStringTemp = menuPropString;
                        menuPropString = String.Empty;

                        if (numberOfProps != 0)
                        {
                            for (int i = 0; i < numberOfProps; i++)
                            {
                                menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";
                            }

                            if (menuPropString != string.Empty)
                            {
                                string header = UTY.GetStringCom(menuPropString);
                                string[] menuProps = UTY.GetStringList(menuPropString);

                                if (header == "end")
                                {
                                    break;
                                }
                                else if (header == "マテリアル変更")
                                {
                                    int matNo = int.Parse(menuProps[2]);
                                    string materialFile = menuProps[3];
                                    modelInfo.MaterialChanges.Add(new MaterialChange(matNo, materialFile));
                                }
                                else if (header == "additem")
                                {
                                    modelInfo.ModelFile = menuProps[1];
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        private static void ProcModScriptBin(byte[] cd, GameObject go)
        {
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8);
            string str1 = binaryReader.ReadString();
            NDebug.Assert(str1 == "CM3D2_MOD", "ProcModScriptBin 例外 : ヘッダーファイルが不正です。" + str1);
            binaryReader.ReadInt32();
            binaryReader.ReadString();
            binaryReader.ReadString();
            binaryReader.ReadString();
            binaryReader.ReadString();
            binaryReader.ReadString();
            string mpnValue = binaryReader.ReadString();
            MPN mpn = MPN.null_mpn;
            try
            {
                mpn = (MPN)Enum.Parse(typeof(MPN), mpnValue);
            }
            catch { }
            if (mpn != MPN.null_mpn)
            {
                binaryReader.ReadString();
            }
            string s = binaryReader.ReadString();
            int num2 = binaryReader.ReadInt32();
            Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
            for (int i = 0; i < num2; i++)
            {
                string key = binaryReader.ReadString();
                int count = binaryReader.ReadInt32();
                byte[] value = binaryReader.ReadBytes(count);
                dictionary.Add(key, value);
            }
            binaryReader.Close();

            using (StringReader stringReader = new StringReader(s))
            {
                IMode mode = IMode.None;
                string slotname = String.Empty;
                Material material = null;
                int num3 = 0;
                string line;
                bool change = false;
                while ((line = stringReader.ReadLine()) != null)
                {
                    string[] array = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (array[0] == "アイテム変更" || array[0] == "マテリアル変更")
                    {
                        mode = IMode.ItemChange;
                    }
                    else if (array[0] == "テクスチャ変更")
                    {
                        mode = IMode.TexChange;
                    }
                    if (mode == IMode.ItemChange)
                    {
                        if (array[0] == "スロット名")
                        {
                            slotname = array[1];
                            change = true;
                        }
                        if (change)
                        {
                            if (array[0] == "マテリアル番号")
                            {
                                num3 = int.Parse(array[1]);
                                foreach (Transform transform in go.GetComponentsInChildren<Transform>(true))
                                {
                                    Renderer component = transform.GetComponent<Renderer>();
                                    if (component != null && component.materials != null)
                                    {
                                        Material[] materials = component.materials;
                                        for (int k = 0; k < materials.Length; k++)
                                        {
                                            if (k == num3)
                                            {
                                                material = materials[k];
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (material != null)
                            {
                                if (array[0] == "テクスチャ設定")
                                {
                                    ChangeTex(num3, array[1], array[2].ToLower(), dictionary, go);
                                }
                                else if (array[0] == "色設定")
                                {
                                    material.SetColor(array[1],
                                        new Color(
                                            float.Parse(array[2]) / 255f,
                                            float.Parse(array[3]) / 255f,
                                            float.Parse(array[4]) / 255f,
                                            float.Parse(array[5]) / 255f
                                        )
                                    );
                                }
                                else if (array[0] == "数値設定")
                                {
                                    material.SetFloat(array[1], float.Parse(array[2]));
                                }
                            }
                        }
                    }
                    else if (mode == IMode.TexChange)
                    {
                        int matno = int.Parse(array[2]);
                        ChangeTex(matno, array[3], array[4].ToLower(), dictionary, go);
                    }
                }
            }
        }

        private static void ChangeTex(
            int matno, string prop, string filename, Dictionary<string, byte[]> matDict, GameObject go
        )
        {
            TextureResource textureResource = null;
            byte[] buf = matDict[filename.ToLowerInvariant()];
            textureResource = new TextureResource(2, 2, TextureFormat.ARGB32, null, buf);
            List<Renderer> list = new List<Renderer>(3);
            go.transform.GetComponentsInChildren<Renderer>(true, list);
            foreach (Renderer r in list)
            {
                if (r != null && r.material != null)
                {
                    if (matno < r.materials.Length)
                    {
                        r.materials[matno].SetTexture(prop, null);
                        Texture2D texture2D = textureResource.CreateTexture2D();
                        texture2D.name = filename;
                        r.materials[matno].SetTexture(prop, texture2D);
                    }
                }
            }
        }

        private static GameObject LoadSkinMesh_R(string modelFileName, int layer)
        {
            byte[] buffer = null;
            using (AFileBase afileBase = GameUty.FileOpen(modelFileName, null))
            {
                if (afileBase.IsValid() && afileBase.GetSize() != 0)
                {
                    buffer = afileBase.ReadAll();
                }
                else
                {
                    Debug.LogError("invalid model");
                    return null;
                }
            }
            using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8))
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("seed")) as GameObject;
                gameObject.layer = 1;
                GameObject gameObject2 = null;
                Hashtable hashtable = new Hashtable();
                string text = binaryReader.ReadString();
                if (text != "CM3D2_MESH")
                {
                    NDebug.Assert("LoadSkinMesh_R 例外 : ヘッダーファイルが不正です。" + text, false);
                }
                int num = binaryReader.ReadInt32();
                string str = binaryReader.ReadString();
                gameObject.name = "_SM_" + str;
                string b = binaryReader.ReadString();
                int num2 = binaryReader.ReadInt32();
                List<GameObject> list = new List<GameObject>();
                for (int i = 0; i < num2; i++)
                {
                    GameObject gameObject3 = UnityEngine.Object.Instantiate(Resources.Load("seed")) as GameObject;
                    gameObject3.layer = layer;
                    gameObject3.name = binaryReader.ReadString();
                    list.Add(gameObject3);
                    if (gameObject3.name == b)
                    {
                        gameObject2 = gameObject3;
                    }
                    hashtable[gameObject3.name] = gameObject3;
                    bool flag = binaryReader.ReadByte() != 0;
                    if (flag)
                    {
                        GameObject gameObject4 = UnityEngine.Object.Instantiate(Resources.Load("seed")) as GameObject;
                        gameObject4.name = gameObject3.name + "_SCL_";
                        gameObject4.transform.parent = gameObject3.transform;
                        hashtable[gameObject3.name + "&_SCL_"] = gameObject4;
                    }
                }
                for (int j = 0; j < num2; j++)
                {
                    int num3 = binaryReader.ReadInt32();
                    if (num3 >= 0)
                    {
                        list[j].transform.parent = list[num3].transform;
                    }
                    else
                    {
                        list[j].transform.parent = gameObject.transform;
                    }
                }
                for (int k = 0; k < num2; k++)
                {
                    Transform transform = list[k].transform;
                    float x = binaryReader.ReadSingle();
                    float y = binaryReader.ReadSingle();
                    float z = binaryReader.ReadSingle();
                    transform.localPosition = new Vector3(x, y, z);
                    float x2 = binaryReader.ReadSingle();
                    float y2 = binaryReader.ReadSingle();
                    float z2 = binaryReader.ReadSingle();
                    float w = binaryReader.ReadSingle();
                    transform.localRotation = new Quaternion(x2, y2, z2, w);
                    if (2001 <= num)
                    {
                        bool flag2 = binaryReader.ReadBoolean();
                        if (flag2)
                        {
                            float x3 = binaryReader.ReadSingle();
                            float y3 = binaryReader.ReadSingle();
                            float z3 = binaryReader.ReadSingle();
                            transform.localScale = new Vector3(x3, y3, z3);
                        }
                    }
                }
                int num4 = binaryReader.ReadInt32();
                int num5 = binaryReader.ReadInt32();
                int num6 = binaryReader.ReadInt32();
                SkinnedMeshRenderer skinnedMeshRenderer = gameObject2.AddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.updateWhenOffscreen = true;
                skinnedMeshRenderer.skinnedMotionVectors = false;
                skinnedMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
                skinnedMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                skinnedMeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                Transform[] array2 = new Transform[num6];
                for (int l = 0; l < num6; l++)
                {
                    string text2 = binaryReader.ReadString();
                    if (!hashtable.ContainsKey(text2))
                    {
                        Debug.LogError("nullbone= " + text2);
                    }
                    else
                    {
                        GameObject gameObject5;
                        if (hashtable.ContainsKey(text2 + "&_SCL_"))
                        {
                            gameObject5 = (GameObject)hashtable[text2 + "&_SCL_"];
                        }
                        else
                        {
                            gameObject5 = (GameObject)hashtable[text2];
                        }
                        array2[l] = gameObject5.transform;
                    }
                }
                skinnedMeshRenderer.bones = array2;
                Mesh mesh = new Mesh();
                skinnedMeshRenderer.sharedMesh = mesh;
                Mesh mesh2 = mesh;
                // bodyskin.listDEL.Add(mesh2);
                Matrix4x4[] array4 = new Matrix4x4[num6];
                for (int m = 0; m < num6; m++)
                {
                    for (int n = 0; n < 16; n++)
                    {
                        array4[m][n] = binaryReader.ReadSingle();
                    }
                }
                mesh2.bindposes = array4;
                Vector3[] array5 = new Vector3[num4];
                Vector3[] array6 = new Vector3[num4];
                Vector2[] array7 = new Vector2[num4];
                BoneWeight[] array8 = new BoneWeight[num4];
                for (int num8 = 0; num8 < num4; num8++)
                {
                    float num9 = binaryReader.ReadSingle();
                    float num10 = binaryReader.ReadSingle();
                    float new_z = binaryReader.ReadSingle();
                    array5[num8].Set(num9, num10, new_z);
                    num9 = binaryReader.ReadSingle();
                    num10 = binaryReader.ReadSingle();
                    new_z = binaryReader.ReadSingle();
                    array6[num8].Set(num9, num10, new_z);
                    num9 = binaryReader.ReadSingle();
                    num10 = binaryReader.ReadSingle();
                    array7[num8].Set(num9, num10);
                }
                mesh2.vertices = array5;
                mesh2.normals = array6;
                mesh2.uv = array7;
                int num11 = binaryReader.ReadInt32();
                if (num11 > 0)
                {
                    Vector4[] array9 = new Vector4[num11];
                    for (int num12 = 0; num12 < num11; num12++)
                    {
                        float x4 = binaryReader.ReadSingle();
                        float y4 = binaryReader.ReadSingle();
                        float z4 = binaryReader.ReadSingle();
                        float w2 = binaryReader.ReadSingle();
                        array9[num12] = new Vector4(x4, y4, z4, w2);
                    }
                    mesh2.tangents = array9;
                }
                for (int num13 = 0; num13 < num4; num13++)
                {
                    array8[num13].boneIndex0 = binaryReader.ReadUInt16();
                    array8[num13].boneIndex1 = binaryReader.ReadUInt16();
                    array8[num13].boneIndex2 = binaryReader.ReadUInt16();
                    array8[num13].boneIndex3 = binaryReader.ReadUInt16();
                    array8[num13].weight0 = binaryReader.ReadSingle();
                    array8[num13].weight1 = binaryReader.ReadSingle();
                    array8[num13].weight2 = binaryReader.ReadSingle();
                    array8[num13].weight3 = binaryReader.ReadSingle();
                }

                mesh2.boneWeights = array8;
                mesh2.subMeshCount = num5;
                for (int num19 = 0; num19 < num5; num19++)
                {
                    int num20 = binaryReader.ReadInt32();
                    int[] array10 = new int[num20];
                    for (int num21 = 0; num21 < num20; num21++)
                    {
                        array10[num21] = (int)binaryReader.ReadUInt16();
                    }
                    mesh2.SetTriangles(array10, num19);
                }
                int num22 = binaryReader.ReadInt32();
                Material[] array11 = new Material[num22];
                for (int num23 = 0; num23 < num22; num23++)
                {
                    Material material = ImportCM.ReadMaterial(binaryReader);
                    array11[num23] = material;
                }
                skinnedMeshRenderer.materials = array11;
                return gameObject;
            }
        }

        public static GameObject LoadModel(string menuFile)
        {
            return LoadModel(new ModItem { MenuFile = menuFile });
        }

        public static GameObject LoadModel(ModItem modItem)
        {
            byte[] menuBuffer = null;
            byte[] modMenuBuffer = null;

            if (modItem.IsOfficialMod)
            {
                using (FileStream fileStream = new FileStream(modItem.MenuFile, FileMode.Open))
                {
                    if (fileStream == null || fileStream.Length == 0)
                    {
                        Debug.LogError("Could not open mod menu");
                        return null;
                    }
                    else
                    {
                        modMenuBuffer = new byte[fileStream.Length];
                        fileStream.Read(modMenuBuffer, 0, (int)fileStream.Length);
                    }
                }
            }

            string menu = modItem.IsOfficialMod ? modItem.BaseMenuFile : modItem.MenuFile;

            using (AFileBase afileBase = GameUty.FileOpen(menu))
            {
                if (afileBase == null || !afileBase.IsValid())
                {
                    Debug.LogError("Could not open menu");
                    return null;
                }
                else if (afileBase.GetSize() == 0)
                {
                    Debug.LogError("Mod menu is empty");
                    return null;
                }
                else
                {
                    menuBuffer = afileBase.ReadAll();
                }
            }

            ModelInfo modelInfo = new ModelInfo();

            if (ProcScriptBin(menuBuffer, modelInfo))
            {
                GameObject gameObject = null;

                try
                {
                    gameObject = LoadSkinMesh_R(modelInfo.ModelFile, 1);
                }
                catch
                {
                    Debug.LogError($"Could not load mesh for '{modItem.MenuFile}'");
                }

                if (gameObject != null)
                {
                    foreach (MaterialChange matChange in modelInfo.MaterialChanges)
                    {
                        foreach (Transform transform in gameObject.transform.GetComponentsInChildren<Transform>(true))
                        {
                            Renderer renderer = transform.GetComponent<Renderer>();
                            if (renderer != null && renderer.material != null
                                && matChange.MaterialIndex < renderer.materials.Length
                            )
                            {
                                renderer.materials[matChange.MaterialIndex] = ImportCM.LoadMaterial(
                                        matChange.MaterialFile, null, renderer.materials[matChange.MaterialIndex]
                                    );
                            }
                        }
                    }

                    if (modItem.IsOfficialMod)
                    {
                        ProcModScriptBin(modMenuBuffer, gameObject);
                    }
                }

                return gameObject;
            }
            else
            {
                Debug.LogWarning($"Could not parse menu file '{modItem.MenuFile}'");
                return null;
            }
        }

        public static bool ParseNativeMenuFile(int menuIndex, ModItem modItem)
        {
            MenuDataBase menuDataBase = GameMain.Instance.MenuDataBase;
            menuDataBase.SetIndex(menuIndex);
            if (menuDataBase.GetBoDelOnly()) return false;
            modItem.Category = menuDataBase.GetCategoryMpnText();
            if (!accMpn.Contains(modItem.Category)) return false;
            modItem.MenuFile = menuDataBase.GetMenuFileName().ToLower();
            if (!ValidBG2MenuFile(modItem.MenuFile)) return false;
            modItem.Name = menuDataBase.GetMenuName();
            modItem.IconFile = menuDataBase.GetIconS();
            modItem.Priority = menuDataBase.GetPriority();
            return true;
        }

        public static bool ParseMenuFile(string menuFile, ModItem modItem)
        {
            if (!ValidBG2MenuFile(menuFile)) return false;

            byte[] buf = null;
            try
            {
                using (AFileBase afileBase = GameUty.FileOpen(menuFile))
                {
                    if (afileBase == null || !afileBase.IsValid() || afileBase.GetSize() == 0) return false;
                    buf = afileBase.ReadAll();
                }
            }
            catch
            {
                Debug.LogError($"Could not read menu file '{menuFile}'");
                return false;
            }

            using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(buf), Encoding.UTF8))
            {
                string menuHeader = binaryReader.ReadString();
                NDebug.Assert(
                    menuHeader == "CM3D2_MENU", "ProcScriptBin 例外 : ヘッダーファイルが不正です。" + menuHeader
                );
                binaryReader.ReadInt32(); // file version
                binaryReader.ReadString(); // txt path
                modItem.Name = binaryReader.ReadString(); // name
                modItem.Category = binaryReader.ReadString(); // category
                if (!accMpn.Contains(modItem.Category)) return false;
                binaryReader.ReadString(); // description
                binaryReader.ReadInt32(); // idk (as long)

                string menuPropString = String.Empty;
                string menuPropStringTemp = String.Empty;

                try
                {
                    while (true)
                    {
                        int numberOfProps = (int)binaryReader.ReadByte();
                        menuPropStringTemp = menuPropString;
                        menuPropString = String.Empty;

                        if (numberOfProps != 0)
                        {
                            for (int i = 0; i < numberOfProps; i++)
                            {
                                menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";
                            }

                            if (menuPropString != string.Empty)
                            {
                                string header = UTY.GetStringCom(menuPropString);
                                string[] menuProps = UTY.GetStringList(menuPropString);

                                if (header == "end")
                                {
                                    break;
                                }
                                else if (header == "icons" || header == "icon")
                                {
                                    modItem.IconFile = menuProps[1];
                                    break;
                                }
                                else if (header == "priority")
                                {
                                    modItem.Priority = float.Parse(menuProps[1]);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not parse menu file '{menuFile}' because {e.Message}");
                    return false;
                }
            }
            return true;
        }

        public static bool ParseModMenuFile(string modMenuFile, ModItem modItem)
        {
            if (!ValidBG2MenuFile(modMenuFile)) return false;

            byte[] buf = null;
            try
            {
                using (FileStream fileStream = new FileStream(modMenuFile, FileMode.Open))
                {
                    if (fileStream == null) return false;
                    if (fileStream.Length == 0L)
                    {
                        Debug.LogError($"Mod menu file '{modMenuFile}' is empty");
                        return false;
                    }
                    buf = new byte[fileStream.Length];
                    fileStream.Read(buf, 0, (int)fileStream.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not read mod menu file '{modMenuFile} because {e.Message}'");
                return false;
            }

            if (buf == null) return false;

            using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(buf), Encoding.UTF8))
            {
                try
                {
                    if (binaryReader.ReadString() != "CM3D2_MOD") return false;
                    else
                    {
                        binaryReader.ReadInt32();
                        string iconName = binaryReader.ReadString();
                        string baseItemPath = binaryReader.ReadString().Replace(":", " ");
                        modItem.BaseMenuFile = Path.GetFileName(baseItemPath);
                        modItem.Name = binaryReader.ReadString();
                        modItem.Category = binaryReader.ReadString();
                        if (!accMpn.Contains(modItem.Category)) return false;
                        binaryReader.ReadString();
                        string mpnValue = binaryReader.ReadString();
                        MPN mpn = MPN.null_mpn;
                        try
                        {
                            mpn = (MPN)Enum.Parse(typeof(MPN), mpnValue, true);
                        }
                        catch
                        {
                            return false;
                        }
                        if (mpn != MPN.null_mpn)
                        {
                            binaryReader.ReadString();
                        }
                        binaryReader.ReadString();
                        int size = binaryReader.ReadInt32();
                        for (int i = 0; i < size; i++)
                        {
                            string key = binaryReader.ReadString();
                            int count = binaryReader.ReadInt32();
                            byte[] data = binaryReader.ReadBytes(count);
                            if (string.Equals(key, iconName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                                tex.LoadImage(data);
                                modItem.Icon = tex;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not parse mod menu file '{modMenuFile}' because {e}");
                    return false;
                }
            }
            return true;
        }

        public static bool ValidBG2MenuFile(ModItem modItem)
        {
            return accMpn.Contains(modItem.Category) && ValidBG2MenuFile(modItem.MenuFile);
        }

        public static bool ValidBG2MenuFile(string menu)
        {
            menu = Path.GetFileNameWithoutExtension(menu).ToLower();
            return !(menu.EndsWith("_del") || menu.Contains("zurashi") || menu.Contains("mekure")
                || menu.Contains("porori") || menu.Contains("moza") || menu.Contains("folder"));
        }

        public abstract class MenuItem
        {
            public string IconFile { get; set; }
            public Texture2D Icon { get; set; }
        }

        public class ModItem : MenuItem
        {
            public string MenuFile { get; set; }
            public string BaseMenuFile { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public float Priority { get; set; }
            public bool IsMod { get; set; }
            public bool IsOfficialMod { get; set; }

            public static ModItem Deserialize(BinaryReader binaryReader)
            {
                return new ModItem()
                {
                    MenuFile = binaryReader.ReadNullableString(),
                    BaseMenuFile = binaryReader.ReadNullableString(),
                    IconFile = binaryReader.ReadNullableString(),
                    Name = binaryReader.ReadNullableString(),
                    Category = binaryReader.ReadNullableString(),
                    Priority = float.Parse(binaryReader.ReadNullableString()),
                    IsMod = binaryReader.ReadBoolean(),
                    IsOfficialMod = binaryReader.ReadBoolean()
                };
            }

            public void Serialize(BinaryWriter binaryWriter)
            {
                if (IsOfficialMod) return;
                binaryWriter.WriteNullableString(MenuFile);
                binaryWriter.WriteNullableString(BaseMenuFile);
                binaryWriter.WriteNullableString(IconFile);
                binaryWriter.WriteNullableString(Name);
                binaryWriter.WriteNullableString(Category);
                binaryWriter.WriteNullableString(Priority.ToString());
                binaryWriter.Write(IsMod);
                binaryWriter.Write(IsOfficialMod);
            }
        }

        public class MyRoomItem : MenuItem
        {
            public int ID { get; set; }
            public string PrefabName { get; set; }
        }

        private class ModelInfo
        {
            public List<MaterialChange> MaterialChanges { get; set; } = new List<MaterialChange>();
            public string ModelFile { get; set; }
        }

        private struct MaterialChange
        {
            public int MaterialIndex { get; }
            public string MaterialFile { get; }

            public MaterialChange(int matno, string matf)
            {
                this.MaterialIndex = matno;
                this.MaterialFile = matf;
            }
        }
    }
}
