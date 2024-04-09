using Newtonsoft.Json;

namespace MeidoPhotoStudio.Plugin.Framework.Serialization.Json;

public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(Vector3);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            var x = 0f;
            var y = 0f;
            var z = 0f;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value;

                    reader.Read();

                    switch (propertyName)
                    {
                        case nameof(Vector3.x):
                            x = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Vector3.y):
                            y = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Vector3.z):
                            z = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    return new Vector3(x, y, z);
                }
            }
        }

        throw new JsonSerializationException($"Unexpected JSON format for {nameof(Vector3)}");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var vector = (Vector3)value;

        writer.WriteStartObject();
        {
            writer.WritePropertyName(nameof(Vector3.x));
            writer.WriteValue(vector.x);

            writer.WritePropertyName(nameof(Vector3.y));
            writer.WriteValue(vector.y);

            writer.WritePropertyName(nameof(Vector3.z));
            writer.WriteValue(vector.z);
        }

        writer.WriteEndObject();
    }
}
