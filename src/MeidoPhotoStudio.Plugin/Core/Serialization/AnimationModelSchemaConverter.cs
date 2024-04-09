using MeidoPhotoStudio.Plugin.Core.Schema.Character;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class AnimationModelSchemaConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(IAnimationModelSchema);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (existingValue is IAnimationModelSchema)
            return existingValue;

        var jsonObject = JObject.Load(reader);

        if (!jsonObject.TryGetValue(nameof(IAnimationModelSchema.Custom), StringComparison.OrdinalIgnoreCase, out var custom))
            throw new InvalidOperationException($"'{nameof(IAnimationModelSchema.Custom)}' is missing");

        var actualType = custom.ToObject<bool>(serializer)
            ? typeof(CustomAnimationSchema)
            : typeof(GameAnimationSchema);

        return jsonObject.ToObject(actualType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        serializer.Serialize(writer, value);
}
