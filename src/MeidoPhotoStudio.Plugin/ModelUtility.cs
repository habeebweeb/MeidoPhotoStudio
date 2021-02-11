using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;

    public static class ModelUtility
    {
        private enum IMode { None, ItemChange, TexChange }

        private static GameObject deploymentObject;

        private static GameObject GetDeploymentObject()
        {
            if (deploymentObject) return deploymentObject;

            if (!(deploymentObject = GameObject.Find("Deployment Object Parent")))
                deploymentObject = new GameObject("Deployment Object Parent");
            return deploymentObject;
        }

        public static GameObject LoadMyRoomModel(MyRoomItem item)
        {
            var data = MyRoomCustom.PlacementData.GetData(item.ID);
            var gameObject = Object.Instantiate(data.GetPrefab());
            if (gameObject)
            {
                var final = new GameObject();
                gameObject.transform.SetParent(final.transform, true);
                final.transform.SetParent(GetDeploymentObject().transform, false);
                return final;
            }

            Utility.LogMessage($"Could not load MyRoomCreative model '{item.PrefabName}'");

            return null;
        }

        public static GameObject LoadBgModel(string bgName)
        {
            var gameObject = GameMain.Instance.BgMgr.CreateAssetBundle(bgName);
            if (!gameObject) gameObject = Resources.Load<GameObject>("BG/" + bgName);
            if (!gameObject) gameObject = Resources.Load<GameObject>("BG/2_0/" + bgName);

            if (gameObject)
            {
                var final = Object.Instantiate(gameObject);
                final.transform.localScale = Vector3.one * 0.1f;
                return final;
            }

            Utility.LogMessage($"Could not load BG model '{bgName}'");

            return null;
        }

        public static GameObject LoadGameModel(string assetName)
        {
            var gameObject = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);
            if (!gameObject) gameObject = Resources.Load<GameObject>("Prefab/" + assetName);
            if (!gameObject) gameObject = Resources.Load<GameObject>("BG/" + assetName);

            if (gameObject)
            {
                var final = Object.Instantiate(gameObject);
                final.transform.localPosition = Vector3.zero;

                Renderer[] renderers = final.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if (renderer && renderer.gameObject.name.Contains("castshadow"))
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                }

                Collider[] colliders = final.GetComponentsInChildren<Collider>();
                foreach (var collider in colliders)
                {
                    if (collider) collider.enabled = false;
                }

                if (final.transform.localScale != Vector3.one)
                {
                    var parent = new GameObject();
                    final.transform.SetParent(parent.transform, true);
                    return parent;
                }

                return final;
            }

            Utility.LogMessage($"Could not load game model '{assetName}'");

            return null;
        }

        public static GameObject LoadMenuModel(string menuFile) => LoadMenuModel(new ModItem(menuFile));

        public static GameObject LoadMenuModel(ModItem modItem)
        {
            var menu = modItem.IsOfficialMod ? modItem.BaseMenuFile : modItem.MenuFile;

            byte[] modelBuffer;

            try { modelBuffer = ReadAFileBase(menu); }
            catch (Exception e)
            {
                Utility.LogError($"Could not read menu file '{menu}' because {e.Message}\n{e.StackTrace}");
                return null;
            }

            if (ProcScriptBin(modelBuffer, out ModelInfo modelInfo))
            {
                if (InstantiateModel(modelInfo.ModelFile, out var finalModel))
                {
                    IEnumerable<Renderer> renderers = GetRenderers(finalModel).ToList();

                    foreach (MaterialChange matChange in modelInfo.MaterialChanges)
                    {
                        foreach (Renderer renderer in renderers)
                        {
                            if (matChange.MaterialIndex < renderer.materials.Length)
                            {
                                renderer.materials[matChange.MaterialIndex] = ImportCM.LoadMaterial(
                                    matChange.MaterialFile, null, renderer.materials[matChange.MaterialIndex]
                                );
                            }
                        }
                    }

                    if (!modItem.IsOfficialMod) return finalModel;

                    try { modelBuffer = ReadOfficialMod(modItem.MenuFile); }
                    catch (Exception e)
                    {
                        Utility.LogError(
                            $"Could not read mod menu file '{modItem.MenuFile}' because {e.Message}\n{e.StackTrace}"
                        );
                        return null;
                    }

                    ProcModScriptBin(modelBuffer, finalModel);

                    return finalModel;
                }
            }

            Utility.LogMessage($"Could not load menu model '{modItem.MenuFile}'");

            return null;
        }

        private static IEnumerable<Renderer> GetRenderers(GameObject gameObject) => gameObject.transform
            .GetComponentsInChildren<Transform>(true)
            .Select(transform => transform.GetComponent<Renderer>())
            .Where(renderer => renderer && renderer.material).ToList();

        private static GameObject CreateSeed() => Object.Instantiate(Resources.Load<GameObject>("seed"));

        private static bool InstantiateModel(string modelFilename, out GameObject modelParent)
        {
            byte[] buffer;

            modelParent = default;

            try { buffer = ReadAFileBase(modelFilename); }
            catch
            {
                Utility.LogError($"Could not load model file '{modelFilename}'");
                return false;
            }

            using var binaryReader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8);

            if (binaryReader.ReadString() != "CM3D2_MESH")
            {
                Utility.LogError($"{modelFilename} is not a model file");
                return false;
            }

            var modelVersion = binaryReader.ReadInt32();
            var modelName = binaryReader.ReadString();

            modelParent = CreateSeed();
            modelParent.layer = 1;
            modelParent.name = "_SM_" + modelName;

            var rootName = binaryReader.ReadString();
            var boneCount = binaryReader.ReadInt32();

            var boneDict = new Dictionary<string, GameObject>();
            var boneList = new List<GameObject>(boneCount);

            GameObject rootBone = null;

            try
            {
                // read bone data
                for (var i = 0; i < boneCount; i++)
                {
                    GameObject bone = CreateSeed();
                    bone.layer = 1;
                    bone.name = binaryReader.ReadString();

                    if (binaryReader.ReadByte() != 0)
                    {
                        GameObject otherBone = CreateSeed();
                        otherBone.name = bone.name + "_SCL_";
                        otherBone.transform.parent = bone.transform;
                        boneDict[bone.name + "$_SCL_"] = otherBone;
                    }

                    boneList.Add(bone);
                    boneDict[bone.name] = bone;

                    if (bone.name == rootName) rootBone = bone;
                }

                for (var i = 0; i < boneCount; i++)
                {
                    var parentIndex = binaryReader.ReadInt32();
                    boneList[i].transform.parent = parentIndex >= 0
                        ? boneList[parentIndex].transform
                        : modelParent.transform;
                }

                for (var i = 0; i < boneCount; i++)
                {
                    Transform transform = boneList[i].transform;
                    transform.localPosition = binaryReader.ReadVector3();
                    transform.localRotation = binaryReader.ReadQuaternion();
                    if (modelVersion >= 2001 && binaryReader.ReadBoolean())
                        transform.localScale = binaryReader.ReadVector3();
                }

                // read mesh data
                var meshRenderer = rootBone.AddComponent<SkinnedMeshRenderer>();
                meshRenderer.updateWhenOffscreen = true;
                meshRenderer.skinnedMotionVectors = false;
                meshRenderer.lightProbeUsage = LightProbeUsage.Off;
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

                Mesh sharedMesh = meshRenderer.sharedMesh = new Mesh();

                var vertCount = binaryReader.ReadInt32();
                var subMeshCount = binaryReader.ReadInt32();
                var meshBoneCount = binaryReader.ReadInt32();

                var meshBones = new Transform[meshBoneCount];
                for (var i = 0; i < meshBoneCount; i++)
                {
                    var boneName = binaryReader.ReadString();

                    if (!boneDict.ContainsKey(boneName))
                        Debug.LogError("nullbone= " + boneName);
                    else
                    {
                        var keyName = boneName + "$_SCL_";
                        GameObject bone = boneDict.ContainsKey(keyName) ? boneDict[keyName] : boneDict[boneName];
                        meshBones[i] = bone.transform;
                    }
                }

                meshRenderer.bones = meshBones;

                var bindPoses = new Matrix4x4[meshBoneCount];
                for (var i = 0; i < meshBoneCount; i++) bindPoses[i] = binaryReader.ReadMatrix4x4();

                sharedMesh.bindposes = bindPoses;

                var vertices = new Vector3[vertCount];
                var normals = new Vector3[vertCount];
                var uv = new Vector2[vertCount];

                for (var i = 0; i < vertCount; i++)
                {
                    vertices[i] = binaryReader.ReadVector3();
                    normals[i] = binaryReader.ReadVector3();
                    uv[i] = binaryReader.ReadVector2();
                }

                sharedMesh.vertices = vertices;
                sharedMesh.normals = normals;
                sharedMesh.uv = uv;

                var tangentCount = binaryReader.ReadInt32();
                if (tangentCount > 0)
                {
                    var tangents = new Vector4[tangentCount];
                    for (var i = 0; i < tangentCount; i++) tangents[i] = binaryReader.ReadVector4();
                    sharedMesh.tangents = tangents;
                }

                var boneWeights = new BoneWeight[vertCount];
                for (var i = 0; i < vertCount; i++)
                {
                    boneWeights[i].boneIndex0 = binaryReader.ReadUInt16();
                    boneWeights[i].boneIndex1 = binaryReader.ReadUInt16();
                    boneWeights[i].boneIndex2 = binaryReader.ReadUInt16();
                    boneWeights[i].boneIndex3 = binaryReader.ReadUInt16();
                    boneWeights[i].weight0 = binaryReader.ReadSingle();
                    boneWeights[i].weight1 = binaryReader.ReadSingle();
                    boneWeights[i].weight2 = binaryReader.ReadSingle();
                    boneWeights[i].weight3 = binaryReader.ReadSingle();
                }

                sharedMesh.boneWeights = boneWeights;
                sharedMesh.subMeshCount = subMeshCount;

                for (var i = 0; i < subMeshCount; i++)
                {
                    var pointCount = binaryReader.ReadInt32();
                    var triangles = new int[pointCount];
                    for (var j = 0; j < pointCount; j++) triangles[j] = binaryReader.ReadUInt16();
                    sharedMesh.SetTriangles(triangles, i);
                }

                // read materials
                var materialCount = binaryReader.ReadInt32();

                var materials = new Material[materialCount];

                for (var i = 0; i < materialCount; i++) materials[i] = ImportCM.ReadMaterial(binaryReader);

                meshRenderer.materials = materials;

                modelParent.AddComponent<Animation>();

                return true;
            }
            catch (Exception e)
            {
                Utility.LogError($"Could not load mesh for '{modelFilename}' because {e.Message}\n{e.StackTrace}");

                foreach (GameObject bone in boneList.Where(bone => bone)) Object.Destroy(bone);

                if (modelParent) Object.Destroy(modelParent);

                modelParent = null;

                return false;
            }
        }

        private static bool ProcScriptBin(byte[] menuBuf, out ModelInfo modelInfo)
        {
            modelInfo = null;
            using var binaryReader = new BinaryReader(new MemoryStream(menuBuf), Encoding.UTF8);

            if (binaryReader.ReadString() != "CM3D2_MENU") return false;

            modelInfo = new ModelInfo();

            binaryReader.ReadInt32(); // file version
            binaryReader.ReadString(); // txt path
            binaryReader.ReadString(); // name
            binaryReader.ReadString(); // category
            binaryReader.ReadString(); // description
            binaryReader.ReadInt32(); // idk (as long)

            try
            {
                while (true)
                {
                    int numberOfProps = binaryReader.ReadByte();
                    var menuPropString = string.Empty;

                    if (numberOfProps != 0)
                    {
                        for (var i = 0; i < numberOfProps; i++)
                        {
                            menuPropString = $"{menuPropString}\"{binaryReader.ReadString()}\"";
                        }

                        if (menuPropString != string.Empty)
                        {
                            var header = UTY.GetStringCom(menuPropString);
                            string[] menuProps = UTY.GetStringList(menuPropString);

                            if (header == "end") break;

                            switch (header)
                            {
                                case "マテリアル変更":
                                {
                                    var matNo = int.Parse(menuProps[2]);
                                    var materialFile = menuProps[3];
                                    modelInfo.MaterialChanges.Add(new MaterialChange(matNo, materialFile));
                                    break;
                                }
                                case "additem":
                                    modelInfo.ModelFile = menuProps[1];
                                    break;
                            }
                        }
                    }
                    else
                        break;
                }
            }
            catch { return false; }

            return true;
        }

        private static void ProcModScriptBin(byte[] cd, GameObject go)
        {
            var matDict = new Dictionary<string, byte[]>();
            string modData;

            using (var binaryReader = new BinaryReader(new MemoryStream(cd), Encoding.UTF8))
            {
                if (binaryReader.ReadString() != "CM3D2_MOD") return;

                binaryReader.ReadInt32();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                var mpnValue = binaryReader.ReadString();
                var mpn = MPN.null_mpn;
                try { mpn = (MPN) Enum.Parse(typeof(MPN), mpnValue, true); }
                catch
                {
                    /* ignored */
                }

                if (mpn != MPN.null_mpn) binaryReader.ReadString();

                modData = binaryReader.ReadString();
                var entryCount = binaryReader.ReadInt32();
                for (var i = 0; i < entryCount; i++)
                {
                    var key = binaryReader.ReadString();
                    var count = binaryReader.ReadInt32();
                    byte[] value = binaryReader.ReadBytes(count);
                    matDict.Add(key, value);
                }
            }

            var mode = IMode.None;
            var materialChange = false;

            Material material = null;
            var materialIndex = 0;

            using var stringReader = new StringReader(modData);

            string line;

            List<Renderer> renderers = null;

            while ((line = stringReader.ReadLine()) != null)
            {
                string[] data = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                switch (data[0])
                {
                    case "アイテム変更":
                    case "マテリアル変更":
                        mode = IMode.ItemChange;
                        break;
                    case "テクスチャ変更":
                        mode = IMode.TexChange;
                        break;
                }

                switch (mode)
                {
                    case IMode.ItemChange:
                    {
                        if (data[0] == "スロット名") materialChange = true;

                        if (materialChange)
                        {
                            if (data[0] == "マテリアル番号")
                            {
                                materialIndex = int.Parse(data[1]);

                                renderers ??= GetRenderers(go).ToList();

                                foreach (Renderer renderer in renderers)
                                {
                                    if (materialIndex < renderer.materials.Length)
                                        material = renderer.materials[materialIndex];
                                }
                            }

                            if (!material) continue;

                            switch (data[0])
                            {
                                case "テクスチャ設定":
                                    ChangeTex(materialIndex, data[1], data[2].ToLower());
                                    break;
                                case "色設定":
                                    material.SetColor(
                                        data[1],
                                        new Color(
                                            float.Parse(data[2]) / 255f, float.Parse(data[3]) / 255f,
                                            float.Parse(data[4]) / 255f, float.Parse(data[5]) / 255f
                                        )
                                    );
                                    break;
                                case "数値設定":
                                    material.SetFloat(data[1], float.Parse(data[2]));
                                    break;
                            }
                        }

                        break;
                    }
                    case IMode.TexChange:
                    {
                        var matno = int.Parse(data[2]);
                        ChangeTex(matno, data[3], data[4].ToLower());
                        break;
                    }
                    case IMode.None: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            void ChangeTex(int matno, string prop, string filename)
            {
                byte[] buf = matDict[filename.ToLowerInvariant()];
                var textureResource = new TextureResource(2, 2, TextureFormat.ARGB32, null, buf);
                renderers ??= GetRenderers(go).ToList();
                foreach (Renderer r in renderers)
                {
                    r.materials[matno].SetTexture(prop, null);
                    Texture2D texture2D = textureResource.CreateTexture2D();
                    texture2D.name = filename;
                    r.materials[matno].SetTexture(prop, texture2D);
                }
            }
        }

        private class ModelInfo
        {
            public List<MaterialChange> MaterialChanges { get; } = new List<MaterialChange>();
            public string ModelFile { get; set; }
        }

        private readonly struct MaterialChange
        {
            public int MaterialIndex { get; }
            public string MaterialFile { get; }

            public MaterialChange(int materialIndex, string materialFile)
            {
                MaterialIndex = materialIndex;
                MaterialFile = materialFile;
            }
        }
    }
}
