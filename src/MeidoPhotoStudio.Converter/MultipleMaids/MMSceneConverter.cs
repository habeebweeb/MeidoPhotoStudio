using System;
using System.IO;
using System.Linq;
using System.Text;

using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MyRoomCustom;
using UnityEngine;

namespace MeidoPhotoStudio.Converter.MultipleMaids;

public static class MMSceneConverter
{
    private const int ClavicleLIndex = 68;
    private const int KankyoMagic = -765;

    private static readonly int[] BodyRotationIndices =
    {
        71, // Hip
        44, // Pelvis
        40, // Spine
        41, // Spine0a
        42, // Spine1
        43, // Spine1a
        57, // Neck
        ClavicleLIndex, // Clavicle L
        69, // Clavicle R
        46, // UpperArm L
        49, // UpperArm R
        47, // ForeArm L
        50, // ForeArm R
        52, // Thigh L
        55, // Thigh R
        53, // Calf L
        56, // Calf R
        92, // Mune L
        94, // Mune R
        93, // MuneSub L
        95, // MuneSub R
        45, // Hand L
        48, // Hand R
        51, // Foot L
        54, // Foot R
    };

    private static readonly int[] BodyRotationIndices64 =
        BodyRotationIndices.Where(rotation => rotation < 64).ToArray();

    private static readonly CameraInfo DefaultCameraInfo = new();
    private static readonly LightProperties DefaultLightProperty = new();

    public static byte[] Convert(string data, bool environment = false)
    {
        var dataSegments = data.Split('_');

        using var memoryStream = new MemoryStream();
        using var dataWriter = new BinaryWriter(memoryStream, Encoding.UTF8);

        if (!environment)
        {
            ConvertMeido(dataSegments, dataWriter);
            ConvertMessage(dataSegments, dataWriter);
            ConvertCamera(dataSegments, dataWriter);
        }

        ConvertLight(dataSegments, dataWriter);
        ConvertEffect(dataSegments, dataWriter);
        ConvertEnvironment(dataSegments, dataWriter);
        ConvertProps(dataSegments, dataWriter);

        dataWriter.Write("END");

        return memoryStream.ToArray();
    }

    public static SceneMetadata GetSceneMetadata(string data, bool environment = false)
    {
        var dataSegments = data.Split('_');
        var strArray2 = dataSegments[1].Split(';');
        var meidoCount = environment ? KankyoMagic : strArray2.Length;

        return new()
        {
            Version = 1,
            Environment = environment,
            MaidCount = meidoCount,
            MMConverted = true,
        };
    }

    private static void ConvertMeido(string[] data, BinaryWriter writer)
    {
        var strArray2 = data[1].Split(';');

        writer.Write(MeidoManager.Header);

        // MeidoManagerSerializer version
        writer.WriteVersion(1);

        var meidoCount = strArray2.Length;

        writer.Write(meidoCount);

        var transformSerializer = Serialization.GetSimple<TransformDTO>();

        foreach (var rawData in strArray2)
        {
            using var memoryStream = new MemoryStream();
            using var tempWriter = new BinaryWriter(memoryStream, Encoding.UTF8);

            var maidData = rawData.Split(':');

            tempWriter.WriteVersion(1);

            transformSerializer.Serialize(
                new()
                {
                    Position = ConversionUtility.ParseVector3(maidData[59]),
                    Rotation = ConversionUtility.ParseEulerAngle(maidData[58]),
                    LocalScale = ConversionUtility.ParseVector3(maidData[60]),
                },
                tempWriter);

            ConvertHead(maidData, tempWriter);
            ConvertBody(maidData, tempWriter);
            ConvertClothing(maidData, tempWriter);

            writer.Write(memoryStream.Length);
            writer.Write(memoryStream.ToArray());
        }

        ConvertGravity(data[0].Split(','), writer);

        static void ConvertHead(string[] maidData, BinaryWriter writer)
        {
            // MeidoSerializer -> Head version
            writer.WriteVersion(1);

            var sixtyFourFlag = maidData.Length is 64;

            // eye direction
            // MM saves eye rotation directly which is garbage data for meido that don't use the same face model.
            // A lot of users associate scenes with specific meido though so keeping the data is desirable.
            var eyeRotationL = Quaternion.identity;
            var eyeRotationR = Quaternion.identity;

            if (!sixtyFourFlag)
            {
                eyeRotationL = ConversionUtility.ParseEulerAngle(maidData[90]);
                eyeRotationR = ConversionUtility.ParseEulerAngle(maidData[91]);
            }

            writer.Write(eyeRotationL);
            writer.Write(eyeRotationR);

            // free look
            if (sixtyFourFlag)
            {
                writer.Write(false);
                writer.Write(new Vector3(0f, 1f, 0f));
            }
            else
            {
                var freeLookData = maidData[64].Split(',');
                var isFreeLook = int.Parse(freeLookData[0]) is 1;

                writer.Write(isFreeLook);

                Vector3 offsetTarget = isFreeLook
                    ? new(float.Parse(freeLookData[2]), 1f, float.Parse(freeLookData[1]))
                    : new(0f, 1f, 0f);

                writer.Write(offsetTarget);
            }

            // HeadEulerAngle is used to save the head's facing rotation
            // MM does not have this data.
            writer.Write(Vector3.zero);

            // head/eye to camera (Not changed by MM so always true)
            writer.Write(true);
            writer.Write(true);

            // face
            var faceValues = maidData[63].Split(',');

            writer.Write(faceValues.Length);

            for (var i = 0; i < faceValues.Length; i++)
            {
                writer.Write(MMConstants.FaceKeys[i]);
                writer.Write(float.Parse(faceValues[i]));
            }
        }

        static void ConvertBody(string[] maidData, BinaryWriter writer)
        {
            // MeidoSerializer -> Body version
            writer.WriteVersion(1);

            var sixtyFourFlag = maidData.Length is 64;

            writer.Write(sixtyFourFlag);

            // finger rotations
            for (var i = 0; i < 40; i++)
                writer.Write(ConversionUtility.ParseEulerAngle(maidData[i]));

            if (!sixtyFourFlag)
            {
                // toe rotations
                for (var i = 0; i < 2; i++)
                    for (var j = 72 + i; j < 90; j += 2)
                        writer.Write(ConversionUtility.ParseEulerAngle(maidData[j]));
            }

            var rotationIndices = sixtyFourFlag ? BodyRotationIndices64 : BodyRotationIndices;

            // body rotations
            foreach (var index in rotationIndices)
            {
                var rotation = Quaternion.identity;
                var data = maidData[index];

                // check special case for ClavicleL
                if (index is ClavicleLIndex)
                {
                    // NOTE: Versions of MM possibly serialized ClavicleL improperly.
                    // At least I think that's what happened otherwise why would they make this check at all.
                    // https://git.coder.horse/meidomustard/modifiedMM/src/master/MultipleMaids/CM3D2/MultipleMaids/Plugin/MultipleMaids.Update.cs#L4355
                    //
                    // Look at the way MM serializes rotations.
                    // https://git.coder.horse/meidomustard/modifiedMM/src/master/MultipleMaids/CM3D2/MultipleMaids/Plugin/MultipleMaids.Update.cs#L2364
                    // It is most definitely possible MM dev missed a component.
                    //
                    // Also why is strArray9.Length == 2 acceptable? If the length were only 2,
                    // float.Parse(strArray9[2]) would throw an index out of range exception???
                    writer.Write(ConversionUtility.TryParseEulerAngle(data, out rotation));
                }
                else
                {
                    rotation = ConversionUtility.ParseEulerAngle(data);
                }

                writer.Write(rotation);
            }

            // hip position
            writer.Write(sixtyFourFlag ? Vector3.zero : ConversionUtility.ParseVector3(maidData[96]));

            Serialization.GetSimple<PoseInfo>().Serialize(PoseInfo.DefaultPose, writer);
        }

        static void ConvertClothing(string[] maidData, BinaryWriter writer)
        {
            // MeidoSerializer -> Clothing version
            writer.WriteVersion(1);

            // MM does not serialize body visibility
            writer.Write(true);

            // MM does not serialize clothing visibility
            for (var i = 0; i < MaidDressingPane.ClothingSlots.Length; i++)
                writer.Write(true);

            // MM does not serialize curling/shift
            writer.Write(false);
            writer.Write(false);
            writer.Write(false);

            // MPN attach props
            var kousokuUpperMenu = string.Empty;
            var kousokuLowerMenu = string.Empty;
            var sixtyFourFlag = maidData.Length is 64;

            if (!sixtyFourFlag)
            {
                var mpnIndex = int.Parse(maidData[65].Split(',')[0]);

                if (mpnIndex is >= 9 and <= 16)
                {
                    var actualIndex = mpnIndex - 9;

                    if (mpnIndex is 12)
                    {
                        kousokuUpperMenu = MMConstants.MpnAttachProps[actualIndex];
                        kousokuLowerMenu = MMConstants.MpnAttachProps[actualIndex - 1];
                    }
                    else if (mpnIndex is 13)
                    {
                        kousokuUpperMenu = MMConstants.MpnAttachProps[actualIndex + 1];
                        kousokuLowerMenu = MMConstants.MpnAttachProps[actualIndex];
                    }
                    else
                    {
                        if (mpnIndex > 13)
                            actualIndex++;

                        var kousokuMenu = MMConstants.MpnAttachProps[actualIndex];

                        if (MMConstants.MpnAttachProps[actualIndex][7] is 'u')
                            kousokuUpperMenu = kousokuMenu;
                        else
                            kousokuLowerMenu = kousokuMenu;
                    }
                }
            }

            writer.Write(!string.IsNullOrEmpty(kousokuUpperMenu));
            writer.Write(kousokuUpperMenu);

            writer.Write(!string.IsNullOrEmpty(kousokuLowerMenu));
            writer.Write(kousokuLowerMenu);

            // hair/skirt gravity
            // If gravity is enabled at all in MM, it affects all maids.
            // So it's like global gravity is enabled which overrides individual maid gravity settings.
            writer.Write(false);
            writer.Write(Vector3.zero);
            writer.Write(false);
            writer.Write(Vector3.zero);
        }

        static void ConvertGravity(string[] data, BinaryWriter writer)
        {
            var softG = new Vector3(float.Parse(data[12]), float.Parse(data[13]), float.Parse(data[14]));
            var hairGravityActive = softG != MMConstants.DefaultSoftG;

            writer.Write(hairGravityActive);

            // an approximation for hair gravity position
            writer.Write(softG * 90f);

            // MM does not serialize skirt gravity
            writer.Write(Vector3.zero);
        }
    }

    private static void ConvertMessage(string[] data, BinaryWriter writer)
    {
        const string newLine = "&kaigyo";

        writer.Write(MessageWindowManager.Header);

        // MessageWindowManagerSerializer version
        writer.WriteVersion(1);

        var showingMessage = false;
        var name = string.Empty;
        var message = string.Empty;
        var strArray3 = data[0].Split(',');

        if (strArray3.Length > 16)
        {
            showingMessage = int.Parse(strArray3[34]) is 1;
            name = strArray3[35];
            message = strArray3[36].Replace(newLine, "\n");

            // MM does not serialize message font size
        }

        writer.Write(showingMessage);
        writer.Write((int)MessageWindowManager.FontBounds.Left);
        writer.Write(name);
        writer.Write(message);
    }

    private static void ConvertCamera(string[] data, BinaryWriter writer)
    {
        writer.Write("CAMERA");

        // CameraManagerSerializer version
        writer.WriteVersion(1);

        // MM only has one camera
        // current camera index
        writer.Write(0);

        // number of camera slots
        writer.Write(1);

        var strArray3 = data[0].Split(',');
        var cameraTargetPos = DefaultCameraInfo.TargetPos;
        var cameraDistance = DefaultCameraInfo.Distance;
        var cameraRotation = DefaultCameraInfo.Angle;

        if (strArray3.Length > 16)
        {
            cameraTargetPos = new(float.Parse(strArray3[27]), float.Parse(strArray3[28]), float.Parse(strArray3[29]));

            cameraDistance = float.Parse(strArray3[30]);

            cameraRotation =
                Quaternion.Euler(float.Parse(strArray3[31]), float.Parse(strArray3[32]), float.Parse(strArray3[33]));
        }

        Serialization.Get<CameraInfo>().Serialize(
            new(cameraTargetPos, cameraRotation, cameraDistance, 35f), writer);
    }

    private static void ConvertLight(string[] data, BinaryWriter writer)
    {
        writer.Write("LIGHT");

        // LightManagerSerializer version
        writer.WriteVersion(1);

        var strArray3 = data[0].Split(',');
        var greaterThan5 = data.Length >= 5;
        var strArray4 = greaterThan5 ? data[2].Split(',') : null;
        var strArray5 = greaterThan5 ? data[3].Split(';') : null;
        var strArray7 = data.Length >= 6 ? data[5].Split(';') : null;
        var numberOfLights = 1 + (strArray5?.Length - 1 ?? 0);

        writer.Write(numberOfLights);

        var lightPropertySerializer = Serialization.Get<LightProperties>();

        /*
         * Light Types
         *   0 = Directional
         *   1 = Spot
         *   2 = Point
         *   3 = Directional (Colour Mode)
         */

        if (strArray3.Length > 16)
        {
            // Main Light
            var spotAngle = float.Parse(strArray3[25]);

            var lightProperty = new LightProperties
            {
                Rotation = Quaternion.Euler(
                    float.Parse(strArray3[21]), float.Parse(strArray3[22]), float.Parse(strArray3[23])),
                Intensity = float.Parse(strArray3[24]),

                // MM uses spotAngle for both range and spotAngle based on which light type is used
                SpotAngle = spotAngle,
                Range = spotAngle / 5f,
                ShadowStrength = strArray4 is null ? 0.098f : float.Parse(strArray4[0]),
                Colour =
                    new(float.Parse(strArray3[18]), float.Parse(strArray3[19]), float.Parse(strArray3[20]), 1f),
            };

            var lightType = int.Parse(strArray3[17]);

            // DragPointLightSerializer version
            writer.WriteVersion(1);

            for (var i = 0; i < 3; i++)
                if (i == lightType || i is 0 && lightType is 3)
                    lightPropertySerializer.Serialize(lightProperty, writer);
                else
                    lightPropertySerializer.Serialize(DefaultLightProperty, writer);

            var lightPosition = strArray7 is null
                ? LightController.DefaultPosition
                : ConversionUtility.ParseVector3(strArray7[0]);

            writer.Write(lightPosition);

            // light type. 3 is colour mode which uses directional light type.
            writer.Write(lightType is 3 ? 0 : lightType);

            // colour mode
            writer.Write(lightType is 3);

            // MM lights cannot be disabled
            writer.Write(false);
        }
        else
        {
            // Just write defaults if missing
            // DragPointLightSerializer version
            writer.WriteVersion(1);

            for (var i = 0; i < 3; i++)
                lightPropertySerializer.Serialize(DefaultLightProperty, writer);

            writer.Write(LightController.DefaultPosition);
            writer.Write(0);
            writer.Write(false);
            writer.Write(false);
        }

        if (strArray5 is null)
            return;

        for (var i = 0; i < strArray5.Length - 1; i++)
        {
            var lightProperties = strArray5[i].Split(',');
            var spotAngle = float.Parse(lightProperties[7]);

            var lightProperty = new LightProperties
            {
                Rotation = Quaternion.Euler(float.Parse(lightProperties[4]), float.Parse(lightProperties[5]), 18f),
                Intensity = float.Parse(lightProperties[6]),
                SpotAngle = spotAngle,
                Range = spotAngle / 5f,

                // MM does not save shadow strength for other lights
                ShadowStrength = 0.098f,
                Colour = new(
                    float.Parse(lightProperties[1]),
                    float.Parse(lightProperties[2]),
                    float.Parse(lightProperties[3]),
                    1f),
            };

            var lightType = int.Parse(lightProperties[0]);

            // DragPointLightSerializer version
            writer.WriteVersion(1);

            for (var j = 0; j < 3; j++)
                lightPropertySerializer.Serialize(j == lightType ? lightProperty : DefaultLightProperty, writer);

            var lightPosition = strArray7 is null
                ? LightController.DefaultPosition
                : ConversionUtility.ParseVector3(strArray7[i + 1]);

            writer.Write(lightPosition);

            // light type. 3 is colour mode which uses directional light type.
            writer.Write(lightType is 3 ? 0 : lightType);

            // colour mode only applies to the main light
            writer.Write(false);

            // MM lights cannot be disabled
            writer.Write(false);
        }
    }

    private static void ConvertEffect(string[] data, BinaryWriter writer)
    {
        if (data.Length < 5)
            return;

        writer.Write(EffectManager.Header);

        // EffectManagerSerializer version
        writer.WriteVersion(1);

        var effectData = data[2].Split(',');

        // bloom
        writer.Write(BloomEffectManager.Header);
        writer.WriteVersion(1);

        writer.Write(int.Parse(effectData[1]) is 1); // active
        writer.Write(float.Parse(effectData[2]) / 5.7f * 100f); // intensity
        writer.Write((int)float.Parse(effectData[3])); // blur iterations

        // bloom threshold colour
        writer.WriteColour(
            new(1f - float.Parse(effectData[4]), 1f - float.Parse(effectData[5]), 1f - float.Parse(effectData[6]), 1f));

        writer.Write(int.Parse(effectData[7]) is 1); // hdr

        // vignetting
        writer.Write(VignetteEffectManager.Header);
        writer.WriteVersion(1);

        writer.Write(int.Parse(effectData[8]) is 1); // active
        writer.Write(float.Parse(effectData[9])); // intensity
        writer.Write(float.Parse(effectData[10])); // blur
        writer.Write(float.Parse(effectData[11])); // blur spread
        writer.Write(float.Parse(effectData[12])); // chromatic aberration

        // blur
        writer.Write(BlurEffectManager.Header);
        writer.WriteVersion(1);

        var blurSize = float.Parse(effectData[13]);

        writer.Write(blurSize > 0f); // active
        writer.Write(blurSize); // blur size

        // Sepia Tone
        writer.Write(SepiaToneEffectManager.Header);
        writer.WriteVersion(1);

        writer.Write(int.Parse(effectData[29]) is 1);

        if (effectData.Length > 15)
        {
            // depth of field
            writer.Write(DepthOfFieldEffectManager.Header);
            writer.WriteVersion(1);

            writer.Write(int.Parse(effectData[15]) is 1); // active
            writer.Write(float.Parse(effectData[16])); // focal length
            writer.Write(float.Parse(effectData[17])); // focal size
            writer.Write(float.Parse(effectData[18])); // aperture
            writer.Write(float.Parse(effectData[19])); // max blur size
            writer.Write(int.Parse(effectData[20]) is 1); // visualize focus

            // fog
            writer.Write(FogEffectManager.Header);
            writer.WriteVersion(1);

            writer.Write(int.Parse(effectData[21]) is 1); // active
            writer.Write(float.Parse(effectData[22])); // fog distance
            writer.Write(float.Parse(effectData[23])); // density
            writer.Write(float.Parse(effectData[24])); // height scale
            writer.Write(float.Parse(effectData[25])); // height

            // fog colour
            writer.WriteColour(
                new(float.Parse(effectData[26]), float.Parse(effectData[27]), float.Parse(effectData[28]), 1f));
        }

        writer.Write(EffectManager.Footer);
    }

    private static void ConvertEnvironment(string[] data, BinaryWriter writer)
    {
        writer.Write("ENVIRONMENT");

        // EnvironmentManagerSerializer version
        writer.WriteVersion(1);

        var environmentData = data[0].Split(',');
        var bgAsset = "Theater";

        if (!int.TryParse(environmentData[2], out _))
            bgAsset = environmentData[2].Replace(' ', '_');

        writer.Write(bgAsset);

        Serialization.GetSimple<TransformDTO>()
            .Serialize(
                new()
                {
                    Position = new(
                        float.Parse(environmentData[6]),
                        float.Parse(environmentData[7]),
                        float.Parse(environmentData[8])),
                    Rotation = Quaternion.Euler(
                        float.Parse(environmentData[3]),
                        float.Parse(environmentData[4]),
                        float.Parse(environmentData[5])),
                    LocalScale = new(
                        float.Parse(environmentData[9]),
                        float.Parse(environmentData[10]),
                        float.Parse(environmentData[11])),
                },
                writer);
    }

    private static void ConvertProps(string[] data, BinaryWriter writer)
    {
        var strArray3 = data[0].Split(',');
        var strArray6 = data.Length >= 5 ? data[4].Split(';') : null;
        var hasWProp = strArray3.Length > 37 && !string.IsNullOrEmpty(strArray3[37]);
        var propCount = strArray6?.Length - 1 ?? 0;

        propCount += hasWProp ? 1 : 0;

        writer.Write(PropManager.Header);

        // PropManagerSerializer version
        writer.WriteVersion(1);

        writer.Write(propCount);

        var propSerializer = Serialization.GetSimple<DragPointPropDTO>();

        if (hasWProp)
        {
            // Props that are spawned by pushing (shift +) W.
            writer.WriteVersion(1);

            var propDto = new DragPointPropDTO
            {
                TransformDTO = new()
                {
                    Position =
                        new(float.Parse(strArray3[41]), float.Parse(strArray3[42]), float.Parse(strArray3[43])),
                    Rotation = Quaternion.Euler(
                        float.Parse(strArray3[38]), float.Parse(strArray3[39]), float.Parse(strArray3[40])),
                    LocalScale =
                        new(float.Parse(strArray3[44]), float.Parse(strArray3[45]), float.Parse(strArray3[46])),
                },
                AttachPointInfo = AttachPointInfo.Empty,
                PropInfo = AssetToPropInfo(strArray3[37]),
                ShadowCasting = false,
            };

            propSerializer.Serialize(propDto, writer);
        }

        if (strArray6 is null)
            return;

        for (var i = 0; i < strArray6.Length - 1; i++)
        {
            var prop = strArray6[i];
            var assetParts = prop.Split(',');
            var propInfo = AssetToPropInfo(assetParts[0]);

            var propDto = new DragPointPropDTO
            {
                PropInfo = propInfo,
                TransformDTO = new()
                {
                    Position =
                        new(float.Parse(assetParts[4]), float.Parse(assetParts[5]), float.Parse(assetParts[6])),
                    Rotation = Quaternion.Euler(
                        float.Parse(assetParts[1]), float.Parse(assetParts[2]), float.Parse(assetParts[3])),
                    LocalScale =
                        new(float.Parse(assetParts[7]), float.Parse(assetParts[8]), float.Parse(assetParts[9])),
                },
                AttachPointInfo = AttachPointInfo.Empty,
                ShadowCasting = propInfo.Type is PropInfo.PropType.Mod,
            };

            propSerializer.Serialize(propDto, writer);
        }

        static PropInfo AssetToPropInfo(string asset)
        {
            const string mmMyRoomPrefix = "creative_";
            const string mm23MyRoomPrefix = "MYR_";
            const string bgOdoguPrefix = "BGodogu";
            const string bgAsPropPrefix = "BG_";

            asset = ConvertSpaces(asset);

            if (asset.StartsWith(mmMyRoomPrefix))
            {
                // modifiedMM my room creative prop
                // modifiedMM serializes the prefabName rather than the ID.
                // Kinda dumb tbh who's idea was this anyway?
                asset = asset.Replace(mmMyRoomPrefix, string.Empty);

                return new(PropInfo.PropType.MyRoom)
                {
                    MyRoomID = MMConstants.MyrAssetNameToData[asset].ID,
                    Filename = asset,
                };
            }

            if (asset.StartsWith(mm23MyRoomPrefix))
            {
                // MM 23.0+ my room creative prop
                var assetID = int.Parse(asset.Replace(mm23MyRoomPrefix, string.Empty));
                var placementData = PlacementData.GetData(assetID);

                var filename = string.IsNullOrEmpty(placementData.assetName)
                    ? placementData.resourceName
                    : placementData.assetName;

                return new(PropInfo.PropType.MyRoom)
                {
                    MyRoomID = assetID,
                    Filename = filename,
                };
            }

            if (asset.Contains('#'))
            {
                if (!asset.Contains(".menu"))
                {
                    // MM's dumb way of using one data structure to store both a human readable name and asset name
                    // ex. 'Pancakes　　　　　　　　　　　　　　　　　　　　#odogu_pancake'
                    return new(PropInfo.PropType.Odogu)
                    {
                        Filename = asset.Split('#')[1],
                    };
                }

                // modifiedMM official COM3D2 mod prop
                var modComponents = asset.Split('#');
                var baseMenuFile = ConvertSpaces(modComponents[0]);
                var modMenuFile = ConvertSpaces(modComponents[1]);

                return new(PropInfo.PropType.Mod)
                {
                    Filename = modMenuFile,
                    SubFilename = baseMenuFile,
                };
            }

            if (asset.EndsWith(".menu"))
            {
                var propType = PropInfo.PropType.Mod;

                // hand items are treated as game props (Odogu) in MPS
                if (asset.StartsWith("handitem", StringComparison.OrdinalIgnoreCase)
                    || asset.StartsWith("kousoku", StringComparison.OrdinalIgnoreCase))
                    propType = PropInfo.PropType.Odogu;

                return new(propType)
                {
                    Filename = asset,
                };
            }

            if (asset.StartsWith(bgOdoguPrefix, StringComparison.OrdinalIgnoreCase))
            {
                // MM prepends BG to certain prop asset names. Don't know why.
                return new(PropInfo.PropType.Odogu)
                {
                    Filename = asset.Substring(2),
                };
            }

            if (asset.StartsWith(bgAsPropPrefix))
            {
                // game bg as prop
                return new(PropInfo.PropType.Bg)
                {
                    Filename = asset.Substring(3),
                };
            }

            return new(PropInfo.PropType.Odogu)
            {
                Filename = asset,
            };
        }

        // MM uses '_' as a separator for different parts of serialized data so it converts all '_' to spaces
        static string ConvertSpaces(string @string) =>
            @string.Replace(' ', '_');
    }
}
