using System.Collections.Generic;
using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class LightManagerSerializer : Serializer<LightManager>
{
    private const short Version = 1;

    private static Serializer<DragPointLight> LightSerializer =>
        Serialization.Get<DragPointLight>();

    public override void Serialize(LightManager manager, BinaryWriter writer)
    {
        writer.Write(LightManager.Header);
        writer.WriteVersion(Version);

        var list = GetLightList(manager);

        writer.Write(list.Count);

        foreach (var light in list)
            LightSerializer.Serialize(light, writer);
    }

    public override void Deserialize(LightManager manager, BinaryReader reader, SceneMetadata metadata)
    {
        manager.ClearLights();

        _ = reader.ReadVersion();

        var lightCount = reader.ReadInt32();
        var list = GetLightList(manager);

        LightSerializer.Deserialize(list[0], reader, metadata);

        for (var i = 1; i < lightCount; i++)
        {
            manager.AddLight();
            LightSerializer.Deserialize(list[i], reader, metadata);
        }
    }

    private static List<DragPointLight> GetLightList(LightManager manager) =>
        Utility.GetFieldValue<LightManager, List<DragPointLight>>(manager, "lightList");
}
