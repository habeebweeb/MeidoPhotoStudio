namespace MeidoPhotoStudio.Plugin;

public class MyRoomItem : MenuItem
{
    public int ID { get; set; }

    public string PrefabName { get; set; }

    public override string ToString() =>
        $"MYR_{ID}#{PrefabName}";
}
