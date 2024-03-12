using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Ionic.Zlib;
using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using MeidoPhotoStudio.Plugin.Core.Schema.Message;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class LegacyDeserializer : ISceneSerializer
{
    internal const short MaximumSupportedVersion = 3;

    [Obsolete($"Use '{nameof(SceneSerializer)}' instead", true)]
    public void SerializeScene(Stream stream, SceneSchema sceneSchema) =>
        throw new NotSupportedException($"{nameof(LegacyDeserializer)} does not support serialization");

    public SceneSchema DeserializeScene(Stream stream) =>
        ParseSceneSchema(stream, out var schema) ? schema : null;

    private static bool ParseSceneSchema(Stream stream, out SceneSchema schema)
    {
        schema = null;

        using var headerReader = new BinaryReader(stream, Encoding.UTF8);

        var sceneHeader = Encoding.UTF8.GetBytes("MPSSCENE");

        if (!Utility.BytesEqual(headerReader.ReadBytes(sceneHeader.Length), sceneHeader))
        {
            Utility.LogError("Not a MPS scene!");

            return false;
        }

        var metadata = new SceneSchemaMetadata(headerReader.ReadInt16())
        {
            Environment = headerReader.ReadBoolean(),
            MaidCount = headerReader.ReadInt32(),
            MMConverted = headerReader.ReadBoolean(),
        };

        if (!CanParseData(metadata))
            return false;

        try
        {
            schema = ParseData(stream, metadata);

            return true;
        }
        catch (Exception e)
        {
            Utility.LogDebug($"Could not deserialize scene because {e}");
        }

        return false;

        static bool CanParseData(SceneSchemaMetadata metadata)
        {
            if (metadata.SceneVersion <= MaximumSupportedVersion)
                return true;

            Utility.LogWarning($"{nameof(LegacyDeserializer)} does not support loading a scene >{MaximumSupportedVersion}");

            return false;
        }

        static SceneSchema ParseData(Stream stream, SceneSchemaMetadata metadata)
        {
            using var decompressedStream = new DeflateStream(stream, CompressionMode.Decompress, true);
            using var reader = new BinaryReader(decompressedStream);

            var environment = metadata.Environment;

            ReadMeidoManagerSchema(reader, metadata);

            var messageWindow = ReadMessageWindowSchema(reader, metadata);
            var camera = ReadCameraSchema(reader, metadata);
            var lights = ReadLightRepositorySchema(reader, metadata);
            var effects = ReadEffectsSchema(reader, metadata);
            var background = ReadBackgroundSchema(reader, metadata);
            var props = ReadPropSchema(reader, metadata);

            return new()
            {
                MessageWindow = messageWindow,
                Camera = camera,
                Lights = lights,
                Effects = effects,
                Background = background,
                Props = props,
            };

            static void ReadMeidoManagerSchema(BinaryReader reader, SceneSchemaMetadata metadata)
            {
                if (metadata.Environment)
                    return;

                _ = reader.ReadString();

                var version = reader.ReadInt16();

                ReadMeidoList(reader, metadata);

                var globalGravityEnabled = reader.ReadBoolean();
                var hairPosition = reader.ReadVector3();
                var skirtPosition = reader.ReadVector3();

                static void ReadMeidoList(BinaryReader reader, SceneSchemaMetadata metadata)
                {
                    var listCount = reader.ReadInt32();

                    for (var i = 0; i < listCount; i++)
                        ReadMeido(reader, metadata);

                    static void ReadMeido(BinaryReader reader, SceneSchemaMetadata metadata)
                    {
                        _ = reader.ReadInt64();

                        var version = reader.ReadInt16();
                        var transform = ReadTransform(reader, metadata);

                        ReadHead(reader, metadata);
                        ReadBody(reader, metadata);
                        ReadClothing(reader, metadata);

                        static void ReadHead(BinaryReader reader, SceneSchemaMetadata metadata)
                        {
                            var version = reader.ReadInt16();
                            var eyeRotationLDelta = reader.ReadQuaternion();
                            var eyeRotationRDelta = reader.ReadQuaternion();
                            var freeLook = reader.ReadBoolean();
                            var offsetLookTarget = reader.ReadVector3();
                            var headAngle = reader.ReadVector3();
                            var headToCam = reader.ReadBoolean();
                            var eyeToCam = reader.ReadBoolean();
                            var face = ReadFaceData(reader, metadata);

                            static Dictionary<string, float> ReadFaceData(BinaryReader reader, SceneSchemaMetadata metadata)
                            {
                                var itemCount = reader.ReadInt32();

                                var dictionary = new Dictionary<string, float>();

                                for (var i = 0; i < itemCount; i++)
                                    dictionary.Add(reader.ReadString(), reader.ReadSingle());

                                return dictionary;
                            }
                        }

                        static void ReadBody(BinaryReader reader, SceneSchemaMetadata metadata)
                        {
                            var version = reader.ReadInt16();

                            var poseBinary = metadata.MMConverted ? null : reader.ReadBytes(reader.ReadInt32());

                            if (metadata.MMConverted)
                                ReadMMPoseSchema(reader, metadata);

                            ReadPoseInfo(reader, metadata);

                            var muneSubL = version < 2 ? Quaternion.identity : reader.ReadQuaternion();
                            var muneSubR = version < 2 ? Quaternion.identity : reader.ReadQuaternion();

                            static void ReadPoseInfo(BinaryReader reader, SceneSchemaMetadata metadata)
                            {
                                var version = reader.ReadInt16();
                                var poseGroup = reader.ReadString();
                                var pose = reader.ReadString();
                                var customPose = reader.ReadBoolean();
                            }

                            static void ReadMMPoseSchema(BinaryReader reader, SceneSchemaMetadata metadata)
                            {
                                var sixtyFourFlag = reader.ReadBoolean();
                                var fingerToeRotations = ReadFingerToeRotations(reader, sixtyFourFlag);
                                var boneRotations = ReadBoneRotations(reader, sixtyFourFlag, out var properClavicle);
                                var hipPosition = reader.ReadVector3();

                                static Quaternion[] ReadFingerToeRotations(BinaryReader reader, bool sixtyFour)
                                {
                                    var boneCount = sixtyFour ? 40 : 58;
                                    var array = new Quaternion[boneCount];

                                    for (var i = 0; i < boneCount; i++)
                                        array[i] = reader.ReadQuaternion();

                                    return array;
                                }

                                static Quaternion[] ReadBoneRotations(BinaryReader reader, bool sixtyFour, out bool properClavicle)
                                {
                                    var boneCount = sixtyFour ? 18 : 25;
                                    var array = new Quaternion[boneCount];

                                    properClavicle = true;

                                    const int CalvicleLIndex = 7;

                                    for (var i = 0; i < boneCount; i++)
                                    {
                                        var rotation = Quaternion.identity;

                                        // Check for ClavicleL
                                        if (!sixtyFour && i == CalvicleLIndex)
                                        {
                                            var properRotation = reader.ReadBoolean();

                                            if (properRotation)
                                                rotation = reader.ReadQuaternion();

                                            properClavicle = properRotation;
                                        }
                                        else
                                        {
                                            rotation = reader.ReadQuaternion();
                                        }

                                        array[i] = rotation;
                                    }

                                    return array;
                                }
                            }
                        }

                        static void ReadClothing(BinaryReader reader, SceneSchemaMetadata metadata)
                        {
                            var version = reader.ReadInt16();
                            var bodyVisible = reader.ReadBoolean();
                            var visibleClothing = ReadVisibleClothing(reader, metadata);
                            var curlingFront = reader.ReadBoolean();
                            var curlingBack = reader.ReadBoolean();
                            var pantsuShift = reader.ReadBoolean();
                            var mpnAttachUpper = reader.ReadBoolean();
                            var mpnAttachUpperName = reader.ReadString();
                            var mpnAttachLower = reader.ReadBoolean();
                            var mpnAttachLowerName = reader.ReadString();
                            var hairGravityActive = reader.ReadBoolean();
                            var hairGravityPosition = reader.ReadVector3();
                            var skirtGravityActive = reader.ReadBoolean();
                            var skirtGravityPosition = reader.ReadVector3();

                            static Dictionary<TBody.SlotID, bool> ReadVisibleClothing(BinaryReader reader, SceneSchemaMetadata metadata) =>
                                MaidDressingPane.ClothingSlots.ToDictionary(slot => slot, _ => reader.ReadBoolean());
                        }
                    }
                }
            }

            static MessageWindowSchema ReadMessageWindowSchema(BinaryReader reader, SceneSchemaMetadata metadata)
            {
                if (metadata.Environment)
                    return null;

                _ = reader.ReadString();

                return new(reader.ReadInt16())
                {
                    ShowingMessage = reader.ReadBoolean(),
                    FontSize = reader.ReadInt32(),
                    Name = reader.ReadString(),
                    MessageBody = reader.ReadString(),
                };
            }

            static CameraSchema ReadCameraSchema(BinaryReader reader, SceneSchemaMetadata metadata)
            {
                if (metadata.Environment)
                    return null;

                _ = reader.ReadString();

                return new(reader.ReadInt16())
                {
                    CurrentCameraSlot = reader.ReadInt32(),
                    CameraInfo = ReadCameraInfoList(reader, metadata),
                };

                static List<CameraInfoSchema> ReadCameraInfoList(BinaryReader reader, SceneSchemaMetadata metadata)
                {
                    var listCount = reader.ReadInt32();

                    var list = new List<CameraInfoSchema>();

                    for (var i = 0; i < listCount; i++)
                        list.Add(ReadCameraInfo(reader, metadata));

                    return list;

                    static CameraInfoSchema ReadCameraInfo(BinaryReader reader, SceneSchemaMetadata metadata) =>
                        new(reader.ReadInt16())
                        {
                            TargetPosition = reader.ReadVector3(),
                            Rotation = reader.ReadQuaternion(),
                            Distance = reader.ReadSingle(),
                            FOV = reader.ReadSingle(),
                        };
                }
            }

            static LightRepositorySchema ReadLightRepositorySchema(BinaryReader reader, SceneSchemaMetadata metadata)
            {
                _ = reader.ReadString();

                return new(reader.ReadInt16())
                {
                    Lights = ReadLightList(reader, metadata),
                };

                static List<LightSchema> ReadLightList(BinaryReader reader, SceneSchemaMetadata metadata)
                {
                    var listCount = reader.ReadInt32();

                    var list = new List<LightSchema>();

                    for (var i = 0; i < listCount; i++)
                        list.Add(ReadLightSchema(reader, metadata));

                    return list;

                    static LightSchema ReadLightSchema(BinaryReader reader, SceneSchemaMetadata metadata)
                    {
                        var version = reader.ReadInt16();

                        return new(version)
                        {
                            DirectionalProperties = ReadLightProperties(reader, metadata),
                            SpotProperties = ReadLightProperties(reader, metadata),
                            PointProperties = ReadLightProperties(reader, metadata),
                            Position = reader.ReadVector3(),
                            Type = MpsLightTypeToLightType(reader.ReadInt32()),
                            ColourMode = reader.ReadBoolean(),
                            Enabled = !reader.ReadBoolean(),
                        };

                        static LightType MpsLightTypeToLightType(int value) =>
                            value switch {
                                0 => LightType.Directional,
                                1 => LightType.Spot,
                                2 => LightType.Point,
                                _ => LightType.Directional,
                            };

                        static LightPropertiesSchema ReadLightProperties(BinaryReader reader, SceneSchemaMetadata metadata) =>
                            new(reader.ReadInt16())
                            {
                                Rotation = reader.ReadQuaternion(),
                                Intensity = reader.ReadSingle(),
                                Range = reader.ReadSingle(),
                                SpotAngle = reader.ReadSingle(),
                                ShadowStrength = reader.ReadSingle(),
                                Colour = reader.ReadColour(),
                            };
                    }
                }
            }

            static EffectsSchema ReadEffectsSchema(BinaryReader reader, SceneSchemaMetadata metadata)
            {
                _ = reader.ReadString();
                var version = reader.ReadInt16();

                BloomSchema bloom = null;
                DepthOfFieldSchema depthOfField = null;
                FogSchema fog = null;
                VignetteSchema vignette = null;
                SepiaToneSchema sepiaTone = null;
                BlurSchema blur = null;

                string header;

                while ((header = reader.ReadString()) is not "END_EFFECT")
                {
                    if (header is "EFFECT_BLOOM")
                        bloom = ReadBloom(reader, metadata);
                    else if (header is "EFFECT_DOF")
                        depthOfField = ReadDepthOfField(reader, metadata);
                    else if (header is "EFFECT_FOG")
                        fog = ReadFog(reader, metadata);
                    else if (header is "EFFECT_VIGNETTE")
                        vignette = ReadVignette(reader, metadata);
                    else if (header is "EFFECT_SEPIA")
                        sepiaTone = ReadSepiaTone(reader, metadata);
                    else if (header is "EFFECT_BLUR")
                        blur = ReadBlur(reader, metadata);
                }

                return new(version)
                {
                    Bloom = bloom,
                    DepthOfField = depthOfField,
                    Fog = fog,
                    Vignette = vignette,
                    SepiaTone = sepiaTone,
                    Blur = blur,
                };

                static BloomSchema ReadBloom(BinaryReader reader, SceneSchemaMetadata metadata) =>
                    new(reader.ReadInt16())
                    {
                        Active = reader.ReadBoolean(),
                        BloomValue = reader.ReadSingle(),
                        BlurIterations = reader.ReadInt32(),
                        BloomThresholdColour = reader.ReadColour(),
                        BloomHDR = reader.ReadBoolean(),
                    };

                static DepthOfFieldSchema ReadDepthOfField(BinaryReader reader, SceneSchemaMetadata metadata) =>
                    new(reader.ReadInt16())
                    {
                        Active = reader.ReadBoolean(),
                        FocalLength = reader.ReadSingle(),
                        FocalSize = reader.ReadSingle(),
                        Aperture = reader.ReadSingle(),
                        MaxBlurSize = reader.ReadSingle(),
                        VisualizeFocus = reader.ReadBoolean(),
                    };

                static FogSchema ReadFog(BinaryReader reader, SceneSchemaMetadata metadata) =>
                    new(reader.ReadInt16())
                    {
                        Active = reader.ReadBoolean(),
                        Distance = reader.ReadSingle(),
                        Density = reader.ReadSingle(),
                        HeightScale = reader.ReadSingle(),
                        Height = reader.ReadSingle(),
                        FogColour = reader.ReadColour(),
                    };

                static VignetteSchema ReadVignette(BinaryReader reader, SceneSchemaMetadata metadata) =>
                    new(reader.ReadInt16())
                    {
                        Active = reader.ReadBoolean(),
                        Intensity = reader.ReadSingle(),
                        Blur = reader.ReadSingle(),
                        BlurSpread = reader.ReadSingle(),
                        ChromaticAberration = reader.ReadSingle(),
                    };

                static SepiaToneSchema ReadSepiaTone(BinaryReader reader, SceneSchemaMetadata metadata) =>
                    new(reader.ReadInt16())
                    {
                        Active = reader.ReadBoolean(),
                    };

                static BlurSchema ReadBlur(BinaryReader reader, SceneSchemaMetadata metadata) =>
                    new(reader.ReadInt16())
                    {
                        Active = reader.ReadBoolean(),
                        BlurSize = reader.ReadSingle(),
                    };
            }

            static BackgroundSchema ReadBackgroundSchema(BinaryReader reader, SceneSchemaMetadata metadata)
            {
                _ = reader.ReadString();
                var version = reader.ReadInt16();

                return new(version)
                {
                    BackgroundName = reader.ReadString(),
                    Transform = ReadTransform(reader, metadata),
                    Colour = Color.black,
                };
            }

            static PropsSchema ReadPropSchema(BinaryReader reader, SceneSchemaMetadata metadata)
            {
                _ = reader.ReadString();

                var version = reader.ReadInt16();
                var propList = ReadPropsList(reader, metadata);

                return new(version)
                {
                    Props = propList.Select(ConvertProp).ToList(),
                    DragHandleSettings = propList.Select(ConvertDragHandleSettings).ToList(),
                    PropAttachment = propList.Select(prop => prop.AttachPoint).ToList(),
                };

                static List<LegacyPropSchema> ReadPropsList(BinaryReader reader, SceneSchemaMetadata metadata)
                {
                    var listCount = reader.ReadInt32();
                    var list = new List<LegacyPropSchema>();

                    for (var i = 0; i < listCount; i++)
                        list.Add(ReadProp(reader, metadata));

                    return list;

                    static LegacyPropSchema ReadProp(BinaryReader reader, SceneSchemaMetadata metadata)
                    {
                        var version = reader.ReadInt16();

                        return new(version)
                        {
                            PropInfo = ReadPropInfo(reader, metadata),
                            Transform = ReadTransform(reader, metadata),
                            AttachPoint = ReadAttachPoint(reader, metadata),
                            ShadowCasting = reader.ReadBoolean(),
                            DragHandleEnabled = version < 2 || reader.ReadBoolean(),
                            GizmoEnabled = version < 2 || reader.ReadBoolean(),
                            GizmoMode = version >= 2
                                ? (CustomGizmo.GizmoMode)reader.ReadInt32()
                                : CustomGizmo.GizmoMode.World,
                            PropVisible = version < 2 || reader.ReadBoolean(),
                        };

                        static PropInfoSchema ReadPropInfo(BinaryReader reader, SceneSchemaMetadata metadata) =>
                            new(reader.ReadInt16())
                            {
                                Type = (PropInfo.PropType)reader.ReadInt32(),
                                Filename = reader.ReadNullableString(),
                                SubFilename = reader.ReadNullableString(),
                                MyRoomID = reader.ReadInt32(),
                                IconFile = reader.ReadNullableString(),
                            };

                        static AttachPointSchema ReadAttachPoint(BinaryReader reader, SceneSchemaMetadata metadata) =>
                            new(reader.ReadInt16())
                            {
                                AttachPoint = (AttachPoint)reader.ReadInt32(),
                                CharacterIndex = reader.ReadInt32(),
                            };
                    }
                }

                static PropControllerSchema ConvertProp(LegacyPropSchema propSchema)
                {
                    return new()
                    {
                        Transform = propSchema.Transform,
                        PropModel = ConvertPropInfo(propSchema.PropInfo),
                        ShadowCasting = propSchema.ShadowCasting,
                        Visible = propSchema.PropVisible,
                    };

                    static IPropModelSchema ConvertPropInfo(PropInfoSchema propInfoSchema) =>
                        propInfoSchema.Type switch
                        {
                            PropInfo.PropType.Mod => new MenuFilePropModelSchema()
                            {
                                ID = string.IsNullOrEmpty(propInfoSchema.SubFilename)
                                    ? propInfoSchema.Filename
                                    : propInfoSchema.SubFilename,
                                Filename = string.IsNullOrEmpty(propInfoSchema.SubFilename)
                                    ? propInfoSchema.Filename
                                    : propInfoSchema.SubFilename,
                            },
                            PropInfo.PropType.MyRoom => new MyRoomPropModelSchema()
                            {
                                ID = propInfoSchema.MyRoomID,
                            },
                            PropInfo.PropType.Bg => new BackgroundPropModelSchema()
                            {
                                ID = propInfoSchema.Filename,
                            },
                            PropInfo.PropType.Odogu when propInfoSchema.Filename.EndsWith(".menu") => new MenuFilePropModelSchema()
                            {
                                ID = propInfoSchema.Filename.ToLower(),
                                Filename = propInfoSchema.Filename.ToLower(),
                            },
                            PropInfo.PropType.Odogu => new OtherPropModelSchema()
                            {
                                AssetName = propInfoSchema.Filename,
                            },
                            _ => null,
                        };
                }

                static DragHandleSchema ConvertDragHandleSettings(LegacyPropSchema propSchema) =>
                    new()
                    {
                        HandleEnabled = propSchema.DragHandleEnabled,
                        GizmoEnabled = propSchema.GizmoEnabled,
                        GizmoSpace = propSchema.GizmoMode,
                    };
            }

            static TransformSchema ReadTransform(BinaryReader reader, SceneSchemaMetadata metadata) =>
                new(reader.ReadInt16())
                {
                    Position = reader.ReadVector3(),
                    Rotation = reader.ReadQuaternion(),
                    LocalPosition = reader.ReadVector3(),
                    LocalRotation = reader.ReadQuaternion(),
                    LocalScale = reader.ReadVector3(),
                };
        }
    }
}
