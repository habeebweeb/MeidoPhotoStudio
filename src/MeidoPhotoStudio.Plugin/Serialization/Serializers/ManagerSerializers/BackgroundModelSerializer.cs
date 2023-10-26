using System.IO;

using MeidoPhotoStudio.Database.Background;

namespace MeidoPhotoStudio.Plugin;

public class BackgroundModelSerializer : SimpleSerializer<BackgroundModel>
{
    private const short Version = 1;

    public override void Serialize(BackgroundModel backgroundModel, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        writer.Write(backgroundModel.ID);
        writer.Write((int)backgroundModel.Category);
        writer.Write(backgroundModel.AssetName);
        writer.Write(backgroundModel.Name);
    }

    public override BackgroundModel Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();
        _ = reader.ReadString();

        return new((BackgroundCategory)reader.ReadInt32(), reader.ReadString(), reader.ReadString());
    }
}
