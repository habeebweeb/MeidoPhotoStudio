using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class HandPresetSerializer
{
    private const int BaseCount = 5;
    private const int JointCount = 3;
    private const int BoneCount = BaseCount * JointCount;

    public HandOrFootPreset Deserialize(Stream stream)
    {
        var xmlReader = XmlReader.Create(stream);

        var root = XElement.Load(xmlReader);

        var rightData = (bool?)root.Element("RightData");
        var base64Data = (string)root.Element("BinaryData");

        if (rightData is not bool right)
            return null;

        if (base64Data is null)
            return null;

        try
        {
            var binaryData = Convert.FromBase64String(base64Data);

            using var memoryStream = new MemoryStream(binaryData);
            using var binaryReader = new BinaryReader(memoryStream, System.Text.Encoding.UTF8);

            var rotations = new Quaternion[BoneCount];

            for (var i = 0; i < BoneCount; i++)
                rotations[i] = binaryReader.ReadQuaternion();

            var type = right ? HandOrFootType.HandRight : HandOrFootType.HandLeft;

            return new(rotations, type);
        }
        catch
        {
            return null;
        }
    }

    public void Serialize(HandOrFootPreset preset, Stream stream)
    {
        _ = preset ?? throw new ArgumentNullException(nameof(preset));

        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream, System.Text.Encoding.UTF8);

        foreach (var rotation in preset)
            binaryWriter.WriteQuaternion(rotation);

        using var xmlWriter = XmlWriter.Create(stream, new() { Indent = true });

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "true"),
            new XComment("MeidoPhotoStudio Hand Preset"),
            new XElement(
                "FingerData",
                new XElement("GameVersion", Misc.GAME_VERSION),
                new XElement("RightData", preset.FromRight),
                new XElement("BinaryData", Convert.ToBase64String(memoryStream.ToArray()))));

        document.Save(xmlWriter);
    }
}
