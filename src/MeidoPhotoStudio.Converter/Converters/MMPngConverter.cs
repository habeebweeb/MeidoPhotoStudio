using System;
using System.IO;
using System.Text;
using MeidoPhotoStudio.Converter.MultipleMaids;
using MeidoPhotoStudio.Converter.Utility;

namespace MeidoPhotoStudio.Converter.Converters
{
    public class MMPngConverter : IConverter
    {
        private static readonly byte[] KankyoHeader = Encoding.ASCII.GetBytes("KANKYO");
        private const string InputDirectoryName = "Input";
        public const string ConverterName = "ModifiedMM PNG";

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

            var thumbnailData = PngUtility.ExtractPng(fileStream);

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
                if (Plugin.Instance == null)
                    return;

                var logger = Plugin.Instance.Logger;
                logger.LogWarning($"Could not decompress scene data from {pngFile} because {e}");

                return;
            }

            if (string.IsNullOrEmpty(sceneData))
                return;

            var convertedData = MMSceneConverter.Convert(sceneData, background);
            var sceneMetadata = MMSceneConverter.GetSceneMetadata(sceneData, background);

            MPSSceneSerializer.SaveToFile(outputFilename, sceneMetadata, convertedData, thumbnailData);
        }
    }
}
