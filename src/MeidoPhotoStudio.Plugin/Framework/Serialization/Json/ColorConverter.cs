using Newtonsoft.Json;

namespace MeidoPhotoStudio.Plugin.Framework.Serialization.Json;

public class ColorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Color);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            var r = 0f;
            var g = 0f;
            var b = 0f;
            var a = 0f;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value;

                    reader.Read();

                    switch (propertyName)
                    {
                        case nameof(Color.r):
                            r = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Color.g):
                            g = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Color.b):
                            b = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Color.a):
                            a = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    return new Color(r, g, b, a);
                }
            }
        }

        throw new JsonSerializationException($"Unexpected JSON format for {nameof(Color)}");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var color = (Color)value;

        writer.WriteStartObject();

        writer.WritePropertyName(nameof(Color.r));
        writer.WriteValue(color.r);

        writer.WritePropertyName(nameof(Color.g));
        writer.WriteValue(color.g);

        writer.WritePropertyName(nameof(Color.b));
        writer.WriteValue(color.b);

        writer.WritePropertyName(nameof(Color.a));
        writer.WriteValue(color.a);

        writer.WriteEndObject();
    }
}
