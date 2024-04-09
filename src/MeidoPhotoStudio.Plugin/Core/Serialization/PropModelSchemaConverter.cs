using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class PropModelSchemaConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(IPropModelSchema);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (existingValue is IPropModelSchema)
            return existingValue;

        var jsonObject = JObject.Load(reader);

        var discriminator = jsonObject.GetValue(nameof(IPropModelSchema.Type), StringComparison.OrdinalIgnoreCase)
            ?? throw new InvalidOperationException("type property is missing.");

        var propTypeString = discriminator.Value<string>();
        var propType = (PropType)Enum.Parse(typeof(PropType), propTypeString, true);

        var propModel = propType switch
        {
            PropType.PhotoBg => typeof(PhotoBgPropModelSchema),
            PropType.Desk => typeof(DeskPropModelSchema),
            PropType.Other => typeof(OtherPropModelSchema),
            PropType.Background => typeof(BackgroundPropModelSchema),
            PropType.MyRoom => typeof(MyRoomPropModelSchema),
            PropType.Menu => typeof(MenuFilePropModelSchema),
            _ => throw new ArgumentOutOfRangeException(nameof(propType)),
        };

        return jsonObject.ToObject(propModel, serializer);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var jsonObject = JObject.FromObject(value, serializer);

        jsonObject.AddFirst(new JProperty(
            nameof(IPropModelSchema.Type),
            Enum.GetName(typeof(PropType), ((IPropModelSchema)value).Type)));

        jsonObject.WriteTo(writer);
    }
}
