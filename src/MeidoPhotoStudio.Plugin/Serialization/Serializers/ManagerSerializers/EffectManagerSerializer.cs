using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MeidoPhotoStudio.Plugin;

public class EffectManagerSerializer : Serializer<EffectManager>
{
    private const short Version = 1;

    public override void Serialize(EffectManager manager, BinaryWriter writer)
    {
        writer.Write(EffectManager.Header);
        writer.WriteVersion(Version);

        foreach (var effectManager in GetEffectManagers(manager).Values)
            Serialization.Get(effectManager.GetType()).Serialize(effectManager, writer);

        writer.Write(EffectManager.Footer);
    }

    public override void Deserialize(EffectManager manager, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var headerToManager =
            GetEffectManagers(manager).ToDictionary(
                x => (string)x.Key.GetField("Header").GetValue(null),
                y => y.Value);

        string header;

        while ((header = reader.ReadString()) is not EffectManager.Footer)
        {
            var effectManager = headerToManager[header];

            Serialization.Get(effectManager.GetType()).Deserialize(effectManager, reader, metadata);
        }
    }

    private static Dictionary<Type, IEffectManager> GetEffectManagers(EffectManager manager) =>
        Utility.GetFieldValue<EffectManager, Dictionary<Type, IEffectManager>>(manager, "effectManagers");
}
