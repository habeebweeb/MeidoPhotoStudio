using System;
using System.IO;
using System.Text;

using MeidoPhotoStudio.Converter.MultipleMaids;
using MeidoPhotoStudio.Converter.Utility;

namespace MeidoPhotoStudio.Converter.Converters;

public class MMPngConverter : IConverter
{
    public const string ConverterName = "ModifiedMM PNG";
    private const string InputDirectoryName = "Input";

    private static readonly byte[] KankyoHeader = Encoding.ASCII.GetBytes("KANKYO");

    public void Convert(string workingDirectory)
    {
        var baseDirectory = Path.Combine(workingDirectory, ConverterName);
        var baseInputDirectory = Path.Combine(baseDirectory, InputDirectoryName);
        var baseOutputDirectory = Path.Combine(baseDirectory, MPSSceneSerializer.FormatDate(DateTime.Now));

        Convert(baseInputDirectory, baseOutputDirectory);
    }

    private static void Convert(string workingDirectory, string destination)
    {
        var directory = new DirectoryInfo(workingDirectory);

        if (!directory.Exists)
            return;

        Directory.CreateDirectory(destination);

        foreach (var file in directory.GetFiles("*.png"))
            ConvertScene(file.FullName, Path.Combine(destination, file.Name));

        foreach (var subDirectory in directory.GetDirectories())
        {
            var subDestination = Path.Combine(destination, subDirectory.Name);

            Convert(subDirectory.FullName, subDestination);
        }
    }

    private static void ConvertScene(string pngFile, string outputFilename)
    {
        var fileStream = File.OpenRead(pngFile);
        var thumbnailData = PngUtility.ExtractPng(fileStream) ?? MPSSceneSerializer.NoThumb;
        var kankyo = new byte[KankyoHeader.Length];
        fileStream.Read(kankyo, 0, KankyoHeader.Length);

        var background = false;

        // ModifiedMM habeebweeb fork scene data uses 'KANKYO' as a header to identify saved environments.
        // Regular scenes will lack a 'KANKYO' header so the filestream position has to be pulled back.
        if (MeidoPhotoStudio.Plugin.Utility.BytesEqual(kankyo, KankyoHeader))
            background = true;
        else
            fileStream.Position -= KankyoHeader.Length;

        string sceneData;

        try
        {
            using var sceneStream = LZMA.Decompress(fileStream);

            sceneData = Encoding.Unicode.GetString(sceneStream.ToArray());
        }
        catch (Exception e)
        {
            if (!Plugin.Instance)
                return;

            if (Plugin.Instance!.Logger is null)
                return;

            Plugin.Instance.Logger.LogWarning($"Could not decompress scene data from {pngFile} because {e}");

            return;
        }

        if (string.IsNullOrEmpty(sceneData))
            return;

        byte[] convertedData;
        MeidoPhotoStudio.Plugin.SceneMetadata sceneMetadata;

        try
        {
            convertedData = MMSceneConverter.Convert(sceneData, background);
            sceneMetadata = MMSceneConverter.GetSceneMetadata(sceneData, background);
        }
        catch (Exception e)
        {
            if (!Plugin.Instance || Plugin.Instance!.Logger is null)
                return;

            Plugin.Instance.Logger.LogError($"Could not convert {pngFile} because {e}");

            return;
        }

        MPSSceneSerializer.SaveToFile(outputFilename, sceneMetadata, convertedData, thumbnailData);
    }
}
