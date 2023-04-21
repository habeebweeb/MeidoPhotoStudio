namespace MeidoPhotoStudio.Plugin;

public readonly struct MaterialChange
{
    public MaterialChange(int materialIndex, string materialFile)
    {
        MaterialIndex = materialIndex;
        MaterialFile = materialFile;
    }

    public int MaterialIndex { get; }

    public string MaterialFile { get; }

    public static MaterialChange Deserialize(System.IO.BinaryReader reader) =>
        new(reader.ReadInt32(), reader.ReadNullableString());

    public void Serialize(System.IO.BinaryWriter writer)
    {
        writer.Write(MaterialIndex);
        writer.WriteNullableString(MaterialFile);
    }
}
