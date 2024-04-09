using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MeidoPhotoStudio.Plugin.Core.Character;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BlendSetSerializer
{
    public FacialExpressionSet Deserialize(Stream stream)
    {
        using var xmlReader = XmlReader.Create(stream);

        return new(
            from element in XElement.Load(xmlReader).Elements()
            let attribute = element.Attribute("name")
            where attribute?.Value is not null
            let hashKey = (string)attribute
            let value = (float?)element ?? 0f
            select new KeyValuePair<string, float>(hashKey, value));
    }

    public void Serialize(FacialExpressionSet expressionSet, Stream stream)
    {
        _ = expressionSet ?? throw new System.ArgumentNullException(nameof(expressionSet));

        using var xmlWriter = XmlWriter.Create(stream, new() { Indent = true });

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "true"),
            new XComment("MeidoPhotoStudio Face Preset"),
            new XElement(
                "FaceData",
                expressionSet.Select(kvp =>
                    new XElement("elm", kvp.Value.ToString("G9"), new XAttribute("name", kvp.Key)))));

        document.Save(xmlWriter);
    }
}
