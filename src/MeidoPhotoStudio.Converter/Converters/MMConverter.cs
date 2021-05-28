using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExIni;
using MeidoPhotoStudio.Converter.MultipleMaids;

namespace MeidoPhotoStudio.Converter.Converters
{
    public class MMConverter : IConverter
    {
        public void Convert(string workingDirectory)
        {
            var baseDirectory = Path.Combine(workingDirectory, MPSSceneSerializer.FormatDate(DateTime.Now));

            foreach (var iniFilePath in GetIniFiles(workingDirectory))
            {
                var section = GetSceneSection(iniFilePath);

                if (section is null)
                    continue;

                var outputDirectoryName = Path.GetFileNameWithoutExtension(iniFilePath);
                var outputDirectory = Path.Combine(baseDirectory, outputDirectoryName);

                Directory.CreateDirectory(outputDirectory);

                var keys = section.Keys.Where(key => !key.Key.StartsWith("ss") && !string.IsNullOrEmpty(key.Value));

                foreach (var key in keys)
                {
                    var background = int.Parse(key.Key.Substring(1)) >= 10000;

                    var convertedData = MMSceneConverter.Convert(key.Value, background);
                    var sceneMetadata = MMSceneConverter.GetSceneMetadata(key.Value, background);

                    var screenshotKey = $"s{key.Key}"; // ex. ss100=thumb_base64
                    string? screenshotBase64 = null;

                    if (section.HasKey(screenshotKey) && !string.IsNullOrEmpty(section[screenshotKey].Value))
                        screenshotBase64 = section[screenshotKey].Value;

                    var filename = GenerateFilename(iniFilePath, key);
                    var fullPath = Path.Combine(outputDirectory, filename);

                    MPSSceneSerializer.SaveToFile(fullPath, sceneMetadata, convertedData, screenshotBase64);
                }
            }
        }

        private static IEnumerable<string> GetIniFiles(string workingDirectory) =>
            Directory.GetFiles(workingDirectory, "*.ini", SearchOption.AllDirectories);

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
