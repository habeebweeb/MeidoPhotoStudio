using System.IO;
using System.Linq;
using System.Text;

using MeidoPhotoStudio.Plugin.Core.Schema;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MPSScene
{
    private SceneSchemaMetadata metadata;

    public MPSScene(string path, Texture2D thumbnail = null)
    {
        FileInfo = new(path);

        if (!thumbnail)
        {
            thumbnail = new(1, 1, TextureFormat.ARGB32, false);
            thumbnail.LoadImage(File.ReadAllBytes(FileInfo.FullName));
        }

        Thumbnail = thumbnail;
    }

    public Texture2D Thumbnail { get; }

    public FileInfo FileInfo { get; }

    public bool Environment { get; private set; }

    public int NumberOfMaids { get; private set; }

    public void Preload()
    {
        if (metadata is not null)
            return;

        using var fileStream = FileInfo.OpenRead();

        Utility.SeekPngEnd(fileStream);

        using var binaryReader = new BinaryReader(fileStream, Encoding.UTF8);

        var sceneHeader = Encoding.UTF8.GetBytes("MPSSCENE");

        if (!binaryReader.ReadBytes(sceneHeader.Length).SequenceEqual(sceneHeader))
        {
            Utility.LogWarning($"'{FileInfo.FullName}' is not a MPS Scene");

            return;
        }

        metadata = new(binaryReader.ReadInt16())
        {
            Environment = binaryReader.ReadBoolean(),
            MaidCount = binaryReader.ReadInt32(),
            MMConverted = binaryReader.ReadBoolean(),
        };

        Environment = metadata.Environment;
        NumberOfMaids = metadata.MaidCount;
    }

    public void Destroy()
    {
        if (Thumbnail)
            Object.DestroyImmediate(Thumbnail);
    }
}
