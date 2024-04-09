using Newtonsoft.Json;

namespace MeidoPhotoStudio.Plugin.Framework.Serialization.Json;

public class QuaternionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Quaternion);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            var x = 0f;
            var y = 0f;
            var z = 0f;
            var w = 0f;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value;

                    reader.Read();

                    switch (propertyName)
                    {
                        case nameof(Quaternion.x):
                            x = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Quaternion.y):
                            y = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Quaternion.z):
                            z = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                        case nameof(Quaternion.w):
                            w = (float)Convert.ChangeType(reader.Value, typeof(float));

                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    return new Quaternion(x, y, z, w);
                }
            }
        }

        throw new JsonSerializationException($"Unexpected JSON format for {nameof(Quaternion)}");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var quaternion = (Quaternion)value;

        writer.WriteStartObject();
        {
            writer.WritePropertyName(nameof(Quaternion.x));
            writer.WriteValue(quaternion.x);

            writer.WritePropertyName(nameof(Quaternion.y));
            writer.WriteValue(quaternion.y);

            writer.WritePropertyName(nameof(Quaternion.z));
            writer.WriteValue(quaternion.z);

            writer.WritePropertyName(nameof(Quaternion.w));
            writer.WriteValue(quaternion.w);
        }

        writer.WriteEndObject();
    }
}
