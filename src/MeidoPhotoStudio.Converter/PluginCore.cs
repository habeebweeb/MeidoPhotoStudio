using MeidoPhotoStudio.Converter.Converters;

namespace MeidoPhotoStudio.Converter;

public class PluginCore(string workingDirectory, params IConverter[] converters)
{
    private readonly IConverter[] converters = converters;

    public string WorkingDirectory { get; set; } = workingDirectory;

    public void Convert()
    {
        Directory.CreateDirectory(WorkingDirectory);

        foreach (var converter in converters)
        {
            try
            {
                converter.Convert(WorkingDirectory);
            }
            catch (Exception e)
            {
                if (!Plugin.Instance)
                    continue;

                Plugin.Instance!.Logger!.LogError($"Could not convert data because {e}");
            }
        }
    }
}
