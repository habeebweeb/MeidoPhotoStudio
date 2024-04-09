using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Menu;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class MenuFilePropInstantiator
{
    public GameObject Instantiate(MenuFilePropModel menuFile, out ShapeKeyController shapeKeyController)
    {
        _ = menuFile ?? throw new ArgumentNullException(nameof(menuFile));

        shapeKeyController = null;

        var model = InstantiateModel(menuFile.ModelFilename, out shapeKeyController);

        if (!model)
            return null;

        ApplyMaterialChanges(menuFile.MaterialChanges, model);
        ApplyModelAnimations(menuFile.ModelAnimations, model);
        ApplyMaterialAnimations(menuFile.ModelMaterialAnimations, model);

        model.name = menuFile.Filename;

        return model;

        static GameObject InstantiateModel(string modelFilename, out ShapeKeyController shapeKeyController)
        {
            shapeKeyController = null;

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

                var blendDatas = new List<BlendData>();

                while (true)
                {
                    var header = reader.ReadString();

                    if (header is "end")
                        break;
                    else if (header is "morph")
                        blendDatas.Add(ReadMorphData(reader));
                }

                if (blendDatas.Count is not 0)
                {
                    shapeKeyController = new(
                        meshRenderer.sharedMesh,
                        new TBodySkin.OriVert
                        {
                            VCount = sharedMesh.vertexCount,
                            nSubMeshCount = subMeshCount,
                            vOriVert = sharedMesh.vertices,
                            vOriNorm = sharedMesh.normals,
                            bwWeight = sharedMesh.boneWeights,
                            nSubMeshOriTri = Enumerable.Range(0, sharedMesh.subMeshCount).Select(sharedMesh.GetTriangles).ToArray(),
                        },
                        [.. blendDatas]);
                }

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

            static BlendData ReadMorphData(BinaryReader reader)
            {
                var hashKey = reader.ReadString();
                var count = reader.ReadInt32();

                var blendData = new BlendData()
                {
                    name = hashKey,
                    vert = new Vector3[count],
                    norm = new Vector3[count],
                    v_index = new int[count],
                };

                for (var i = 0; i < count; i++)
                {
                    blendData.v_index[i] = reader.ReadUInt16();
                    blendData.vert[i] = reader.ReadVector3();
                    blendData.norm[i] = reader.ReadVector3();
                }

                return blendData;
            }

            static GameObject CreateSeed() =>
                Object.Instantiate(Resources.Load<GameObject>("seed"));
        }

        static void ApplyMaterialChanges(IEnumerable<MaterialChange> materialChanges, GameObject model)
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
