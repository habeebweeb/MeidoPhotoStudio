using System;
using System.IO;
using System.Text;

namespace MeidoPhotoStudio.Plugin;

public class SceneSerializer
{
    public static readonly byte[] SceneHeader = Encoding.UTF8.GetBytes("MPSSCENE");

    private const short SceneVersion = 3;
    private const int KankyoMagic = -765;

    private readonly MeidoManager meidoManager;
    private readonly MessageWindowManager messageWindowManager;
    private readonly CameraManager cameraManager;
    private readonly LightManager lightManager;
    private readonly EffectManager effectManager;
    private readonly EnvironmentManager environmentManager;
    private readonly PropManager propManager;

    public SceneSerializer(
        MeidoManager meidoManager,
        MessageWindowManager messageWindowManager,
        CameraManager cameraManager,
        LightManager lightManager,
        EffectManager effectManager,
        EnvironmentManager environmentManager,
        PropManager propManager)
    {
        this.meidoManager = meidoManager;
        this.messageWindowManager = messageWindowManager;
        this.cameraManager = cameraManager;
        this.lightManager = lightManager;
        this.effectManager = effectManager;
        this.environmentManager = environmentManager;
        this.propManager = propManager;
    }

    public byte[] SerializeScene(bool environment = false)
    {
        if (meidoManager.Busy)
            return null;

        try
        {
            using var memoryStream = new MemoryStream();
            using var headerWriter = new BinaryWriter(memoryStream, Encoding.UTF8);

            headerWriter.Write(SceneHeader);

            new SceneMetadata
            {
                Version = SceneVersion,
                Environment = environment,
                MaidCount = environment ? KankyoMagic : meidoManager.ActiveMeidoList.Count,
                MMConverted = false,
            }.WriteMetadata(headerWriter);

            using var compressionStream = memoryStream.GetCompressionStream();
            using var dataWriter = new BinaryWriter(compressionStream, Encoding.UTF8);

            if (!environment)
            {
                Serialization.Get<MeidoManager>().Serialize(meidoManager, dataWriter);
                Serialization.Get<MessageWindowManager>().Serialize(messageWindowManager, dataWriter);
                Serialization.Get<CameraManager>().Serialize(cameraManager, dataWriter);
            }

            Serialization.Get<LightManager>().Serialize(lightManager, dataWriter);
            Serialization.Get<EffectManager>().Serialize(effectManager, dataWriter);
            Serialization.Get<EnvironmentManager>().Serialize(environmentManager, dataWriter);
            Serialization.Get<PropManager>().Serialize(propManager, dataWriter);

            dataWriter.Write("END");

            compressionStream.Close();

            var data = memoryStream.ToArray();

            return data;
        }
        catch (Exception e)
        {
            Utility.LogError($"Failed to save scene because {e.Message}\n{e.StackTrace}");

            return null;
        }
    }

    public void DeserializeScene(byte[] data)
    {
        if (meidoManager.Busy)
        {
            Utility.LogMessage("Could not apply scene. Meidos are Busy");

            return;
        }

        using var memoryStream = new MemoryStream(data);
        using var headerReader = new BinaryReader(memoryStream, Encoding.UTF8);

        if (!Utility.BytesEqual(headerReader.ReadBytes(SceneHeader.Length), SceneHeader))
        {
            Utility.LogError("Not a MPS scene!");

            return;
        }

        var metadata = SceneMetadata.ReadMetadata(headerReader);

        if (metadata.Version > SceneVersion)
        {
            Utility.LogWarning("Cannot load scene. Scene is too new.");
            Utility.LogWarning($"Your version: {SceneVersion}, Scene version: {metadata.Version}");

            return;
        }

        using var uncompressed = memoryStream.Decompress();
        using var dataReader = new BinaryReader(uncompressed, Encoding.UTF8);

        var header = string.Empty;
        var previousHeader = string.Empty;

        try
        {
            while ((header = dataReader.ReadString()) is not "END")
            {
                switch (header)
                {
                    case MeidoManager.Header:
                        Serialization.Get<MeidoManager>().Deserialize(meidoManager, dataReader, metadata);

                        break;
                    case MessageWindowManager.Header:
                        Serialization.Get<MessageWindowManager>()
                            .Deserialize(messageWindowManager, dataReader, metadata);

                        break;
                    case CameraManager.Header:
                        Serialization.Get<CameraManager>().Deserialize(cameraManager, dataReader, metadata);

                        break;
                    case LightManager.Header:
                        Serialization.Get<LightManager>().Deserialize(lightManager, dataReader, metadata);

                        break;
                    case EffectManager.Header:
                        Serialization.Get<EffectManager>().Deserialize(effectManager, dataReader, metadata);

                        break;
                    case EnvironmentManager.Header:
                        Serialization.Get<EnvironmentManager>().Deserialize(environmentManager, dataReader, metadata);

                        break;
                    case PropManager.Header:
                        Serialization.Get<PropManager>().Deserialize(propManager, dataReader, metadata);

                        break;
                    default:
                        throw new Exception($"Unknown header '{header}'");
                }

                previousHeader = header;
            }
        }
        catch (Exception e)
        {
            Utility.LogError(
                $"Failed to deserialize scene because {e.Message}\nCurrent header: '{header}'. " +
                $"Last header: '{previousHeader}'");

            Utility.LogError(e.StackTrace);
        }
    }
}
