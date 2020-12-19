using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class SceneMetadata
    {
        public short Version { get; init; }
        public bool Environment { get; init; }
        public bool MMConverted { get; init; }
        public int MaidCount { get; init; }


        public void WriteMetadata(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Environment);
            writer.Write(MaidCount);
            writer.Write(MMConverted);
        }

        public static SceneMetadata ReadMetadata(BinaryReader reader)
        {
            return new()
            {
                Version = reader.ReadVersion(),
                Environment = reader.ReadBoolean(),
                MaidCount = reader.ReadInt32(),
                MMConverted = reader.ReadBoolean()
            };
        }

        public void Deconstruct(
            out short version, out bool environment, out bool mmConverted, out int maidCount
        )
        {
            version = Version;
            environment = Environment;
            mmConverted = MMConverted;
            maidCount = MaidCount;
        }
    }
}
