using MeidoPhotoStudio.Plugin.Core.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class ExtensionDataConverter : JsonConverter
{
    private readonly Dictionary<string, Type> extensionTypeMap = [];

    public void RegisterExtensionType(string extensionID, Type type)
    {
        if (string.IsNullOrEmpty(extensionID))
            throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID));

        _ = type ?? throw new ArgumentNullException(nameof(type));

        if (!typeof(IExtensionData).IsAssignableFrom(type))
            throw new InvalidExtensionDataTypeException($"'{type}' is not assignable to '{nameof(IExtensionData)}'");

        if (extensionTypeMap.ContainsKey(extensionID))
        {
            Plugin.Logger.LogWarning($"Schema type '{type}' is already mapped to an extension with the ID '{extensionID}'");

            return;
        }

        extensionTypeMap[extensionID] = type;
    }

    public void DeregisterExtensionType(string extensionID)
    {
        if (string.IsNullOrEmpty(extensionID))
            throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID));

        extensionTypeMap.Remove(extensionID);
    }

    public override bool CanConvert(Type objectType) =>
        objectType == typeof(IExtensionData);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (existingValue is IExtensionData)
            return existingValue;

        var jsonObject = JObject.Load(reader);

        if (!jsonObject.TryGetValue(nameof(IExtensionData.ExtensionID), StringComparison.OrdinalIgnoreCase, out var idToken))
            throw new InvalidOperationException($"'{nameof(IExtensionData.ExtensionID)}' is missing");

        var id = idToken.Value<string>();

        if (!extensionTypeMap.TryGetValue(id, out var type))
            return null;

        return jsonObject.ToObject(type, serializer);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        serializer.Serialize(writer, value);
}
