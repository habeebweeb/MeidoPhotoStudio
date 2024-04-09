using MeidoPhotoStudio.Plugin.Core.Schema.Character;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class BlendSetModelSchemaConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(IBlendSetModelSchema);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (existingValue is IBlendSetModelSchema)
            return existingValue;

        var jsonObject = JObject.Load(reader);

        if (!jsonObject.TryGetValue(nameof(IBlendSetModelSchema.Custom), StringComparison.OrdinalIgnoreCase, out var custom))
            throw new InvalidOperationException($"'{nameof(IBlendSetModelSchema.Custom)}' is missing");

        var actualType = custom.ToObject<bool>(serializer)
            ? typeof(CustomBlendSetSchema)
            : typeof(GameBlendSetSchema);

        return jsonObject.ToObject(actualType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        serializer.Serialize(writer, value);
}
