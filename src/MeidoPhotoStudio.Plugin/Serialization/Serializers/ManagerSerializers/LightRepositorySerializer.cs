using System.IO;

using MeidoPhotoStudio.Plugin.Core.Lighting;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class LightRepositorySerializer : Serializer<LightRepository>
{
    public const string Header = "LIGHT";

    private const short Version = 1;

    public override void Serialize(LightRepository lightRepository, BinaryWriter writer)
    {
        writer.Write(Header);
        writer.WriteVersion(Version);
        writer.Write(lightRepository.Count);

        foreach (var lightController in lightRepository)
            Serialization.Get<LightController>().Serialize(lightController, writer);
    }

    public override void Deserialize(LightRepository lightRepository, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var lightCount = reader.ReadInt32();

        lightRepository.RemoveAllLights();

        for (var i = 0; i < lightCount; i++)
        {
            if (i is 0)
                lightRepository.AddLight(GameMain.Instance.MainLight.GetComponent<Light>());
            else
                lightRepository.AddLight();

            Serialization.Get<LightController>().Deserialize(lightRepository[i], reader, metadata);
        }
    }
}
