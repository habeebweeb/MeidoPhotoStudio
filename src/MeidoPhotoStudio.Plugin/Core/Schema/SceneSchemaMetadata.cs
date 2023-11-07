namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class SceneSchemaMetadata
{
    public SceneSchemaMetadata(short sceneVersion = SceneSchema.SchemaVersion) =>
        SceneVersion = sceneVersion;

    public short SceneVersion { get; }

    public bool Environment { get; init; }

    public int MaidCount { get; init; }

    public bool MMConverted { get; init; }
}
