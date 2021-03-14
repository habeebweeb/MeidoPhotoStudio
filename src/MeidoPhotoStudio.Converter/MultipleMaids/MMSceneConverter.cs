using System.IO;
using System.Text;
using MeidoPhotoStudio.Plugin;
using UnityEngine;

namespace MeidoPhotoStudio.Converter.MultipleMaids
{
    public static class MMSceneConverter
    {
        private static readonly int[] bodyRotations =
        {
            71, 44, 40, 41, 42, 43, 57, 68, 69, 46, 49, 47, 50, 52, 55, 53, 56, 92, 94, 93, 95, 45, 48, 51, 54,
        };
        private static SimpleSerializer<PoseInfo> PoseInfoSerializer => Serialization.GetSimple<PoseInfo>();
        private static SimpleSerializer<TransformDTO> TransformDtoSerializer => Serialization.GetSimple<TransformDTO>();

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

        private static void ConvertMeido(string[] data, BinaryWriter writer)
        {
            var strArray2 = data[1].Split(';');

            writer.Write(MeidoManager.header);
            writer.WriteVersion(1);

            var meidoCount = strArray2.Length;

            writer.Write(meidoCount);

            var gravityEnabled = false;

            var transformSerializer = TransformDtoSerializer;

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
                    }, writer
                );

                ConvertHead(maidData, tempWriter);
                ConvertBody(maidData, tempWriter);
                ConvertClothing(maidData, tempWriter);
            
                writer.Write(memoryStream.Length);
                writer.Write(memoryStream.ToArray());
            }

            static void ConvertHead(string[] maidData, BinaryWriter writer)
            {
                writer.WriteVersion(1);

                // eye direction
                writer.Write(Quaternion.identity);
                writer.Write(Quaternion.identity);

                // free look
                if (maidData.Length == 64)
                {
                    writer.Write(false);
                    writer.Write(new Vector3(0f, 1f, 0f));
                }
                else
                {
                    var freeLookData = maidData[64].Split(',');
                    var isFreeLook = int.Parse(freeLookData[0]) == 1;

                    writer.Write(isFreeLook);

                    var offsetTarget = isFreeLook
                        ? new(float.Parse(freeLookData[2]), 1f, float.Parse(freeLookData[1]))
                        : new Vector3(0f, 1f, 0f);

                    writer.Write(offsetTarget);
                }

                // HeadEulerAngle is unknown and also ignored for converted scenes
                writer.Write(Vector3.zero);

                // head/eye to camera (Not changed by MM so always true)
                writer.Write(true);
                writer.Write(true);

                // face
                var faceValues = maidData[63].Split(',');
                writer.Write(faceValues.Length);

                for (var i = 0; i < MMConstants.FaceKeys.Length - 1; i++)
                {
                    writer.Write(MMConstants.FaceKeys[i]);
                    writer.Write(float.Parse(faceValues[i]));
                }

                // nosefook
                if (faceValues.Length > 35)
                    writer.Write(float.Parse(faceValues[35]));
            }

            static void ConvertBody(string[] maidData, BinaryWriter writer)
            {
                writer.WriteVersion(1);

                for (var i = 0; i < 40; i++)
                    writer.Write(ConversionUtility.ParseEulerAngle(maidData[i]));
            }

            static void ConvertClothing(string[] maidData, BinaryWriter writer)
            {
                writer.WriteVersion(1);
            }
        }

        private static void ConvertMessage(string[] data, BinaryWriter writer) { }

        private static void ConvertCamera(string[] data, BinaryWriter writer) { }

        private static void ConvertLight(string[] data, BinaryWriter writer) { }

        private static void ConvertEffect(string[] data, BinaryWriter writer) { }

        private static void ConvertEnvironment(string[] data, BinaryWriter writer) { }

        private static void ConvertProps(string[] data, BinaryWriter writer) { }
    }
}
