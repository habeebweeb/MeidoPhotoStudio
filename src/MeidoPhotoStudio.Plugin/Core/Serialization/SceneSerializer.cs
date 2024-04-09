using Ionic.Zlib;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Framework.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class SceneSerializer : ISceneSerializer
{
    private const string SceneMagic = "MPSSCENE";

    private static JsonSerializer Serializer =>
        JsonSerializer.Create(new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverterCollection()
            {
                new ColorConverter(),
                new Vector3Converter(),
                new Vector2Converter(),
                new QuaternionConverter(),
                new PropModelSchemaConverter(),
                new AnimationModelSchemaConverter(),
                new BlendSetModelSchemaConverter(),
            },
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        });

    public void SerializeScene(Stream stream, SceneSchema sceneSchema)
    {
        using var headerWriter = new BinaryWriter(stream, Encoding.UTF8);

        headerWriter.Write(Encoding.UTF8.GetBytes(SceneMagic));

        headerWriter.Write(SceneSchema.SchemaVersion);
        headerWriter.Write(false);

        headerWriter.Write(sceneSchema.Character.Characters.Count);
        headerWriter.Write(false);

        using var compressionStream = new DeflateStream(stream, CompressionMode.Compress);
        using var streamWriter = new StreamWriter(compressionStream, Encoding.UTF8);
        using var jsonWriter = new JsonTextWriter(streamWriter);

        Serializer.Serialize(jsonWriter, sceneSchema);
    }

    public SceneSchema DeserializeScene(Stream stream)
    {
        using var headerReader = new BinaryReader(stream, Encoding.UTF8);

        var sceneHeader = Encoding.UTF8.GetBytes(SceneMagic);

        if (!headerReader.ReadBytes(sceneHeader.Length).SequenceEqual(sceneHeader))
        {
            Utility.LogError("Not a MPS scene!");

            return null;
        }

        var metadata = new SceneSchemaMetadata(headerReader.ReadInt16())
        {
            Environment = headerReader.ReadBoolean(),
            MaidCount = headerReader.ReadInt32(),
            MMConverted = headerReader.ReadBoolean(),
        };

        if (metadata.SceneVersion > SceneSchema.SchemaVersion)
        {
            Utility.LogWarning("Cannot load scene. Scene is too new.");
            Utility.LogWarning($"Your version: {SceneSchema.SchemaVersion}, Scene version: {metadata.SceneVersion}");

            return null;
        }

        using var decompressionStream = new DeflateStream(stream, CompressionMode.Decompress);
        using var streamReader = new StreamReader(decompressionStream, Encoding.UTF8);
        using var jsonReader = new JsonTextReader(streamReader);

        var schema = Serializer.Deserialize<SceneSchema>(jsonReader);

        return schema;
    }
}
