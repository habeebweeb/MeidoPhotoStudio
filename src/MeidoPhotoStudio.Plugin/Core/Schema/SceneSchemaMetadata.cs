namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class SceneSchemaMetadata(short sceneVersion = SceneSchema.SchemaVersion)
{
    public short SceneVersion { get; } = sceneVersion;

    public bool Environment { get; init; }

    public int MaidCount { get; init; }

    public bool MMConverted { get; init; }
}
