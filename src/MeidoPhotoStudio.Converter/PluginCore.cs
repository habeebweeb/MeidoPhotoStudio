using MeidoPhotoStudio.Converter.Converters;

namespace MeidoPhotoStudio.Converter;

public class PluginCore
{
    private readonly IConverter[] converters;

    public PluginCore(string workingDirectory, params IConverter[] converters)
    {
        WorkingDirectory = workingDirectory;

        this.converters = converters;
    }

    public string WorkingDirectory { get; set; }

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
