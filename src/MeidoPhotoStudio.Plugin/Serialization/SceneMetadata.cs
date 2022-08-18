using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class SceneMetadata
{
    public short Version { get; set; }

    public bool Environment { get; set; }

    public int MaidCount { get; set; }

    public bool MMConverted { get; set; }

    public static SceneMetadata ReadMetadata(BinaryReader reader) =>
        new()
        {
            Version = reader.ReadVersion(),
            Environment = reader.ReadBoolean(),
            MaidCount = reader.ReadInt32(),
            MMConverted = reader.ReadBoolean(),
        };

    public void WriteMetadata(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.Write(Environment);
        writer.Write(MaidCount);
        writer.Write(MMConverted);
    }

    public void Deconstruct(out short version, out bool environment, out int maidCount, out bool mmConverted)
    {
        version = Version;
        environment = Environment;
        mmConverted = MMConverted;
        maidCount = MaidCount;
    }
}
