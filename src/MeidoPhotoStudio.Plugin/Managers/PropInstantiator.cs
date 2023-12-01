using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Menu;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropInstantiator
{
    private static GameObject myRoomDeploymentObject;

    public GameObject Instantiate(IPropModel propModel) =>
        propModel is null
            ? throw new System.ArgumentNullException(nameof(propModel))
            : propModel switch
            {
                BackgroundPropModel backgroundPropModel => Instantiate(backgroundPropModel),
                DeskPropModel deskPropModel => Instantiate(deskPropModel),
                MyRoomPropModel myRoomPropModel => Instantiate(myRoomPropModel),
                OtherPropModel otherPropModel => Instantiate(otherPropModel),
                PhotoBgPropModel photoBgPropModel => Instantiate(photoBgPropModel),
                MenuFilePropModel menuFile => Instantiate(menuFile),
                _ => throw new System.NotImplementedException($"'{propModel.GetType()}' is not implemented"),
            };

    private static GameObject InstantiatePrefab(string prefabName)
    {
        var unityObject = Resources.Load<GameObject>($"Prefab/{prefabName}");

        return unityObject ? Object.Instantiate(unityObject) : null;
    }

    private static GameObject InstantiateAssetBundle(string assetName)
    {
        var gameObject = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);

        return gameObject ? Object.Instantiate(gameObject) : null;
    }

    private GameObject Instantiate(BackgroundPropModel propModel)
    {
        var gameObject = propModel.Category is BackgroundCategory.MyRoomCustom
            ? InstantiateMyRoomBackground(propModel.AssetName)
            : InstantiateGameBackground(propModel.AssetName);

        return gameObject;

        static GameObject InstantiateMyRoomBackground(string assetName) =>
            MyRoomCustom.CreativeRoomManager.InstantiateRoom(assetName);

        static GameObject InstantiateGameBackground(string assetName)
        {
            var unityObject = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);

            if (!unityObject)
                unityObject = Resources.Load<GameObject>($"BG/{assetName}");

            if (!unityObject)
                unityObject = Resources.Load<GameObject>($"BG/2_0/{assetName}");

            return unityObject ? Object.Instantiate(unityObject) : null;
        }
    }

    private GameObject Instantiate(DeskPropModel propModel)
    {
        GameObject gameObject = null;

        if (!string.IsNullOrEmpty(propModel.PrefabName))
            gameObject = InstantiatePrefab(propModel.PrefabName);
        else if (!string.IsNullOrEmpty(propModel.AssetName))
            gameObject = InstantiateAssetBundle(propModel.AssetName);

        return gameObject;
    }

    private GameObject Instantiate(MyRoomPropModel propModel)
    {
        var prefab = MyRoomCustom.PlacementData.GetData(propModel.ID).GetPrefab();

        if (!prefab)
            return null;

        var gameObject = Object.Instantiate(prefab);

        if (!gameObject)
            return null;

        var prop = WrapProp(gameObject);

        ParentToDeploymentObject(prop);

        return prop;

        static GameObject WrapProp(GameObject gameObject)
        {
            var container = new GameObject(gameObject.name);

            gameObject.transform.SetParent(container.transform, true);

            return container;
        }

        static void ParentToDeploymentObject(GameObject prop)
        {
            prop.transform.SetParent(GetDeploymentObject().transform, false);

            static GameObject GetDeploymentObject()
            {
                if (myRoomDeploymentObject)
                    return myRoomDeploymentObject;

                var foundParent = GameObject.Find("Deployment Object Parent");

                return myRoomDeploymentObject = foundParent ? foundParent : new GameObject("Deployment Object Parent");
            }
        }
    }

    private GameObject Instantiate(OtherPropModel propModel)
    {
        var gameObject = InstantiateAssetBundle(propModel.AssetName);

        if (!gameObject)
            gameObject = InstantiatePrefab(propModel.AssetName);

        return gameObject;
    }

    private GameObject Instantiate(PhotoBgPropModel propModel)
    {
        var assetName = string.IsNullOrEmpty(propModel.PrefabName)
            ? propModel.AssetName
            : propModel.PrefabName;

        if (string.IsNullOrEmpty(assetName))
            return null;

        // NOTE: Mod/MaidLoader allows for adding custom props so Prefab/AssetName cannot be trusted and the prop has to
        // be spawned by brute force.
        var gameObject = InstantiateAssetBundle(assetName);

        if (!gameObject)
            gameObject = InstantiatePrefab(assetName);

        return gameObject;
    }

    private GameObject Instantiate(MenuFilePropModel menuFile)
    {
        var model = InstantiateModel(menuFile.ModelFilename);

        if (!model)
            return null;

        ApplyMaterialChanges(menuFile.MaterialChanges, model);
        ApplyModelAnimations(menuFile.ModelAnimations, model);
        ApplyMaterialAnimations(menuFile.ModelMaterialAnimations, model);

        model.name = menuFile.Filename;

        return model;

        static GameObject InstantiateModel(string modelFilename)
        {
            using var aFileBase = GameUty.FileOpen(modelFilename);

            if (!aFileBase.IsValid() || aFileBase.GetSize() is 0)
                return null;

            using var aFileBaseStream = new AFileBaseStream(aFileBase);
            using var reader = new BinaryReader(aFileBaseStream, Encoding.UTF8);

            if (reader.ReadString() is not "CM3D2_MESH")
                return null;

            var modelVersion = reader.ReadInt32();
            var modelName = reader.ReadString();

            var modelParent = CreateSeed();

            modelParent.layer = 1;
            modelParent.name = "_SM_" + modelName;

            var rootName = reader.ReadString();
            var boneCount = reader.ReadInt32();
            var boneDict = new Dictionary<string, GameObject>();
            var boneList = new List<GameObject>(boneCount);

            GameObject rootBone = null;

            try
            {
                // read bone data
                for (var i = 0; i < boneCount; i++)
                {
                    var bone = CreateSeed();

                    bone.layer = 1;
                    bone.name = reader.ReadString();

                    if (reader.ReadByte() is not 0)
                    {
                        var otherBone = CreateSeed();

                        otherBone.name = bone.name + "_SCL_";
                        otherBone.transform.parent = bone.transform;
                        boneDict[bone.name + "$_SCL_"] = otherBone;
                    }

                    boneList.Add(bone);
                    boneDict[bone.name] = bone;

                    if (bone.name == rootName)
                        rootBone = bone;
                }

                for (var i = 0; i < boneCount; i++)
                {
                    var parentIndex = reader.ReadInt32();

                    boneList[i].transform.parent = parentIndex >= 0
                        ? boneList[parentIndex].transform
                        : modelParent.transform;
                }

                for (var i = 0; i < boneCount; i++)
                {
                    var transform = boneList[i].transform;

                    transform.localPosition = reader.ReadVector3();
                    transform.localRotation = reader.ReadQuaternion();

                    if (modelVersion >= 2001 && reader.ReadBoolean())
                        transform.localScale = reader.ReadVector3();
                }

                // read mesh data
                var meshRenderer = rootBone.AddComponent<SkinnedMeshRenderer>();

                meshRenderer.updateWhenOffscreen = true;
                meshRenderer.skinnedMotionVectors = false;
                meshRenderer.lightProbeUsage = LightProbeUsage.Off;
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

                var sharedMesh = meshRenderer.sharedMesh = new();
                var vertCount = reader.ReadInt32();
                var subMeshCount = reader.ReadInt32();
                var meshBoneCount = reader.ReadInt32();
                var meshBones = new Transform[meshBoneCount];

                for (var i = 0; i < meshBoneCount; i++)
                {
                    var boneName = reader.ReadString();

                    if (!boneDict.ContainsKey(boneName))
                    {
                        Debug.LogError("nullbone= " + boneName);
                    }
                    else
                    {
                        var keyName = boneName + "$_SCL_";
                        var bone = boneDict.ContainsKey(keyName) ? boneDict[keyName] : boneDict[boneName];

                        meshBones[i] = bone.transform;
                    }
                }

                meshRenderer.bones = meshBones;

                var bindPoses = new Matrix4x4[meshBoneCount];

                for (var i = 0; i < meshBoneCount; i++)
                    bindPoses[i] = reader.ReadMatrix4x4();

                sharedMesh.bindposes = bindPoses;

                var vertices = new Vector3[vertCount];
                var normals = new Vector3[vertCount];
                var uv = new Vector2[vertCount];

                for (var i = 0; i < vertCount; i++)
                {
                    vertices[i] = reader.ReadVector3();
                    normals[i] = reader.ReadVector3();
                    uv[i] = reader.ReadVector2();
                }

                sharedMesh.vertices = vertices;
                sharedMesh.normals = normals;
                sharedMesh.uv = uv;

                var tangentCount = reader.ReadInt32();

                if (tangentCount > 0)
                {
                    var tangents = new Vector4[tangentCount];

                    for (var i = 0; i < tangentCount; i++)
                        tangents[i] = reader.ReadVector4();

                    sharedMesh.tangents = tangents;
                }

                var boneWeights = new BoneWeight[vertCount];

                for (var i = 0; i < vertCount; i++)
                {
                    boneWeights[i].boneIndex0 = reader.ReadUInt16();
                    boneWeights[i].boneIndex1 = reader.ReadUInt16();
                    boneWeights[i].boneIndex2 = reader.ReadUInt16();
                    boneWeights[i].boneIndex3 = reader.ReadUInt16();
                    boneWeights[i].weight0 = reader.ReadSingle();
                    boneWeights[i].weight1 = reader.ReadSingle();
                    boneWeights[i].weight2 = reader.ReadSingle();
                    boneWeights[i].weight3 = reader.ReadSingle();
                }

                sharedMesh.boneWeights = boneWeights;
                sharedMesh.subMeshCount = subMeshCount;

                for (var i = 0; i < subMeshCount; i++)
                {
                    var pointCount = reader.ReadInt32();
                    var triangles = new int[pointCount];

                    for (var j = 0; j < pointCount; j++)
                        triangles[j] = reader.ReadUInt16();

                    sharedMesh.SetTriangles(triangles, i);
                }

                // read materials
                var materialCount = reader.ReadInt32();

                var materials = new Material[materialCount];

                for (var i = 0; i < materialCount; i++)
                    materials[i] = ImportCM.ReadMaterial(reader);

                meshRenderer.materials = materials;

                modelParent.AddComponent<Animation>();

                return modelParent;
            }
            catch
            {
                foreach (var bone in boneList.Where(bone => bone))
                    Object.Destroy(bone);

                if (modelParent)
                    Object.Destroy(modelParent);

                return null;
            }

            static GameObject CreateSeed() =>
                Object.Instantiate(Resources.Load<GameObject>("seed"));
        }

        static void ApplyMaterialChanges(IEnumerable<Database.Props.Menu.MaterialChange> materialChanges, GameObject model)
        {
            var renderers = model.transform
                .GetComponentsInChildren<Transform>(true)
                .Select(transform => transform.GetComponent<Renderer>())
                .Where(renderer => renderer && renderer.material)
                .ToList();

            foreach (var materialChange in materialChanges)
                foreach (var renderer in renderers)
                    if (materialChange.MaterialIndex < renderer.materials.Length)
                        renderer.materials[materialChange.MaterialIndex] = ImportCM.LoadMaterial(
                            materialChange.MaterialFilename, null, renderer.materials[materialChange.MaterialIndex]);
        }

        static void ApplyModelAnimations(IEnumerable<ModelAnimation> modelAnimations, GameObject model)
        {
            var animation = model.GetOrAddComponent<Animation>();

            foreach (var modelAnimation in modelAnimations)
            {
                LoadAnimation(animation, modelAnimation.AnimationName);
                PlayAnimation(animation, modelAnimation.AnimationName, modelAnimation.Loop);
            }

            static Animation LoadAnimation(Animation animation, string animationName)
            {
                if (!animation.GetClip(animationName))
                {
                    var animationFilename = animationName;

                    if (string.IsNullOrEmpty(Path.GetExtension(animationName)))
                        animationFilename += ".anm";

                    var animationClip = ImportCM.LoadAniClipNative(GameUty.FileSystem, animationFilename, true, true, true);

                    if (!animationClip)
                        return animation;

                    animation.Stop();
                    animation.AddClip(animationClip, animationName);
                    animation.clip = animationClip;
                    animation.playAutomatically = true;
                }

                animation.Stop();

                return animation;
            }

            static void PlayAnimation(Animation animation, string animationName, bool loop)
            {
                if (!animation)
                    return;

                animation.Stop();
                animation.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

                if (!animation.GetClip(animationName))
                    return;

                animation[animationName].time = 0f;
                animation.Play(animationName);
            }
        }

        static void ApplyMaterialAnimations(IEnumerable<ModelMaterialAnimation> modelMaterialAnimations, GameObject model)
        {
            var renderer = model.GetComponentInChildren<Renderer>();

            if (!renderer)
                return;

            // TODO: This doesn't make sense. This will just override the previous material animations.
            // TODO: Find a mod that has multiple "animematerial" tags to test.
            foreach (var modelMaterialAnimation in modelMaterialAnimations)
            {
                var materialAnimator = renderer.gameObject.GetOrAddComponent<MaterialAnimator>();

                materialAnimator.m_nMateNo = modelMaterialAnimation.MaterialIndex;
                materialAnimator.Init();
            }
        }
    }
}
