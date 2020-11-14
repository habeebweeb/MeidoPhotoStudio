using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public static partial class MenuFileUtility
    {
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
                    
                    if (!boneDict.ContainsKey(boneName)) Debug.LogError("nullbone= " + boneName);
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
    }
}
