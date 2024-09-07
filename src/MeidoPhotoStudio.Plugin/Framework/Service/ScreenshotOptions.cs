namespace MeidoPhotoStudio.Plugin.Framework.Service;

public readonly record struct ScreenshotOptions(bool CaptureMessageBox, bool CaptureUI)
{
    public ScreenshotOptions()
        : this(true, false)
    {
    }
}
