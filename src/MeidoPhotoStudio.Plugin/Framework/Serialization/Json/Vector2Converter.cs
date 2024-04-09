using Newtonsoft.Json;

namespace MeidoPhotoStudio.Plugin.Framework.Serialization.Json;

public class Vector2Converter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(Vector2);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            var x = 0f;
            var y = 0f;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value;

                    reader.Read();

                    switch (propertyName)
                    {
                        case nameof(Vector2.x):
                            x = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Vector2.y):
                            y = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    return new Vector2(x, y);
                }
            }
        }

        throw new JsonSerializationException($"Unexpected JSON format for {nameof(Vector2)}");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var vector = (Vector2)value;

        writer.WriteStartObject();
        {
            writer.WritePropertyName(nameof(Vector2.x));
            writer.WriteValue(vector.x);

            writer.WritePropertyName(nameof(Vector2.y));
            writer.WriteValue(vector.y);
        }

        writer.WriteEndObject();
    }
}
