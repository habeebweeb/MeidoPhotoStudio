namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class ColourPickerButtonEventArgs(Color colour) : EventArgs
{
    public Color Colour { get; } = colour;
}
