using MeidoPhotoStudio.Plugin.Core.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class ShapeKeyRangeSerializer : IShapeKeyRangeSerializer
{
    private readonly string shapeKeyRangeFilePath;

    public ShapeKeyRangeSerializer(string shapeKeyRangeFilePath)
    {
        if (string.IsNullOrEmpty(shapeKeyRangeFilePath))
            throw new ArgumentException($"'{nameof(shapeKeyRangeFilePath)}' cannot be null or empty.", nameof(shapeKeyRangeFilePath));

        this.shapeKeyRangeFilePath = shapeKeyRangeFilePath;
    }

    private static JsonSerializer Serializer =>
        JsonSerializer.Create(new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        });

    public void Serialize(Dictionary<string, ShapeKeyRange> ranges)
    {
        using var fileStream = File.Create(shapeKeyRangeFilePath);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        using var jsonWriter = new JsonTextWriter(streamWriter);

        Serializer.Serialize(jsonWriter, ranges);
    }

    public Dictionary<string, ShapeKeyRange> Deserialize()
    {
        try
        {
            using var fileStream = File.OpenRead(shapeKeyRangeFilePath);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            using var jsonReader = new JsonTextReader(streamReader);

            return Serializer.Deserialize<Dictionary<string, ShapeKeyRange>>(jsonReader);
        }
        catch
        {
            return [];
        }
    }
}
