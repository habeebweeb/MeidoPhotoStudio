using System;

namespace MeidoPhotoStudio.Plugin;

public class PresetChangeEventArgs : EventArgs
{
    public PresetChangeEventArgs(string path, string category)
    {
        Path = path;
        Category = category;
    }

    public static new PresetChangeEventArgs Empty { get; } = new(string.Empty, string.Empty);

    public string Category { get; }

    public string Path { get; }
}
