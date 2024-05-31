namespace MeidoPhotoStudio.Plugin;

public readonly record struct ScreenshotOptions(bool CaptureMessageBox, bool CaptureUI)
{
    public ScreenshotOptions()
        : this(true, false)
    {
    }
}
