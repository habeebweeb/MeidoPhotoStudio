using MeidoPhotoStudio.Plugin.Core.Schema;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class WrappedSerializer : ISceneSerializer
{
    private readonly LegacyDeserializer legacyDeserializer;
    private readonly SceneSerializer sceneSerializer;

    public WrappedSerializer(SceneSerializer sceneSerializer, LegacyDeserializer legacyDeserializer)
    {
        this.sceneSerializer = sceneSerializer ?? throw new ArgumentNullException(nameof(sceneSerializer));
        this.legacyDeserializer = legacyDeserializer ?? throw new ArgumentNullException(nameof(legacyDeserializer));
    }

    public void SerializeScene(Stream stream, SceneSchema sceneSchema) =>
        sceneSerializer.SerializeScene(stream, sceneSchema);

    public SceneSchema DeserializeScene(Stream stream)
    {
        var startingPosition = stream.Position;

        using var binaryReader = new BinaryReader(stream, Encoding.UTF8);

        var sceneHeader = Encoding.UTF8.GetBytes("MPSSCENE");

        if (!binaryReader.ReadBytes(sceneHeader.Length).SequenceEqual(sceneHeader))
        {
            Utility.LogError("Not a MPS scene!");

            return null;
        }

        var version = binaryReader.ReadInt16();

        stream.Position = startingPosition;

        return version > LegacyDeserializer.MaximumSupportedVersion
            ? sceneSerializer.DeserializeScene(stream)
            : legacyDeserializer.DeserializeScene(stream);
    }
}
