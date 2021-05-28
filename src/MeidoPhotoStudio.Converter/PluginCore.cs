using System;
using System.IO;
using MeidoPhotoStudio.Converter.Converters;

namespace MeidoPhotoStudio.Converter
{
    public class PluginCore
    {
        private readonly IConverter[] converters;
        public string WorkingDirectory { get; set; }

        public PluginCore(string workingDirectory, params IConverter[] converters)
        {
            WorkingDirectory = workingDirectory;
            this.converters = converters;
        }

        public void Convert()
        {
            if (!Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);

            try
            {
                foreach (var converter in converters)
                    converter.Convert(WorkingDirectory);
            }
            catch (Exception e)
            {
                if (Plugin.Instance is not null)
                {
                    var logger = Plugin.Instance.Logger;
                    logger.LogWarning($"Could not convert data because {e.Message}");
                    logger.LogMessage(e.StackTrace);
                }
            }
        }
    }
}
