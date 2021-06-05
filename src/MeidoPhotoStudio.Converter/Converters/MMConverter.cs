using System;
using System.IO;
using System.Linq;
using ExIni;
using MeidoPhotoStudio.Converter.MultipleMaids;

namespace MeidoPhotoStudio.Converter.Converters
{
    public class MMConverter : IConverter
    {
        private const string InputDirectoryName = "Input";
        public const string ConverterName = "MultipleMaids";

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

            foreach (var iniFile in directory.GetFiles("*.ini"))
                ConvertIniFile(iniFile, destination);

            foreach (var subDirectory in directory.GetDirectories())
            {
                var subDestination = Path.Combine(destination, subDirectory.Name);
                Convert(subDirectory.FullName, subDestination);
            }
        }

        private static void ConvertIniFile(FileInfo iniFile, string destination)
        {
            var section = GetSceneSection(iniFile.FullName);

            if (section is null)
                return;

            var outputDirectory = Path.Combine(destination, Path.GetFileNameWithoutExtension(iniFile.Name));

            Directory.CreateDirectory(outputDirectory);

            foreach (var key in section.Keys.Where(
                key => !key.Key.StartsWith("ss") && !string.IsNullOrEmpty(key.Value)
            ))
                ConvertScene(section, key, Path.Combine(outputDirectory, GenerateFilename(iniFile.Name, key)));
        }

        private static void ConvertScene(IniSection section, IniKey key, string filePath)
        {
            var background = int.Parse(key.Key.Substring(1)) >= 10000;

            var convertedData = MMSceneConverter.Convert(key.Value, background);
            var sceneMetadata = MMSceneConverter.GetSceneMetadata(key.Value, background);

            var screenshotKey = $"s{key.Key}"; // ex. ss100=thumb_base64
            string? screenshotBase64 = null;

            if (section.HasKey(screenshotKey) && !string.IsNullOrEmpty(section[screenshotKey].Value))
                screenshotBase64 = section[screenshotKey].Value;

            MPSSceneSerializer.SaveToFile(filePath, sceneMetadata, convertedData, screenshotBase64);
        }

        private static string GenerateFilename(string iniFilePath, IniKey sceneKey)
        {
            var background = int.Parse(sceneKey.Key.Substring(1)) >= 10000;

            var iniFilename = Path.GetFileNameWithoutExtension(iniFilePath);

            var sceneName = sceneKey.Key;

            var data = sceneKey.Value;
            var date = DateTime.Parse(data.Substring(0, data.IndexOf(',')));

            var sceneDate = MPSSceneSerializer.FormatDate(date);

            return $"mm{(background ? "kankyo" : "scene")}_{iniFilename}_{sceneName}_{sceneDate}.png";
        }

        private static IniSection? GetSceneSection(string filePath)
        {
            IniFile iniFile;

            try
            {
                iniFile = IniFile.FromFile(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not {(e is IOException ? "read" : "parse")} ini file {filePath}");
                return null;
            }

            if (!iniFile.HasSection("scene"))
            {
                Console.WriteLine($"{filePath} is not a valid MM config because '[scene]' section is missing");
                return null;
            }

            return iniFile.GetSection("scene");
        }
    }
}
