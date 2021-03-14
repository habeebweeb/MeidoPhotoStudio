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
        private readonly string workingDirectory;

        public MMConverter(string directory) => workingDirectory = directory;

        public void Convert()
        {
            if (!Directory.Exists(workingDirectory))
                Directory.CreateDirectory(workingDirectory);

            foreach (var section in GetSceneSections(workingDirectory))
            {
                foreach (var key in section.Keys.Where(
                    key => !key.Key.StartsWith("ss") && !string.IsNullOrEmpty(key.Value)
                ))
                {
                    var data = key.Value;
                    var screenshotKey = $"s{key.Key}";
                    string? screenshotBase64 = null;

                    if (section.HasKey(screenshotKey) && !string.IsNullOrEmpty(section[screenshotKey].Value))
                        screenshotBase64 = section[screenshotKey].Value;

                    var convertedData = MMSceneConverter.Convert(data);
                    
                }
            }
        }

        private static void Convert(MMScene scene) { }

        private static IEnumerable<IniSection> GetSceneSections(string directory) =>
            Directory.GetFiles(directory, "*.ini", SearchOption.AllDirectories)
                .Select(GetSceneSection)
                .Where(section => section is not null)
                .Select(section => section!);

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
