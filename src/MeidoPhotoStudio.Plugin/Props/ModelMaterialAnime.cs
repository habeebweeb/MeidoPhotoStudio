namespace MeidoPhotoStudio.Plugin;

public readonly struct ModelMaterialAnime
{
    public ModelMaterialAnime(TBody.SlotID slot, int materialNumber)
    {
        Slot = slot;
        MaterialNumber = materialNumber;
    }

    public TBody.SlotID Slot { get; }

    public int MaterialNumber { get; }

    public static ModelMaterialAnime Deserialize(System.IO.BinaryReader reader)
    {
        var slot = (TBody.SlotID)System.Enum.Parse(typeof(TBody.SlotID), reader.ReadString(), true);
        var materialNumber = reader.ReadInt32();

        return new(slot, materialNumber);
    }

    public void Serialize(System.IO.BinaryWriter writer)
    {
        writer.Write(Slot.ToString());
        writer.Write(MaterialNumber);
    }
}
