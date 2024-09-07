using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework.Input;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core;

public class ScreenshotServiceInputHandler(ScreenshotService screenshotService, InputConfiguration inputConfiguration)
    : IInputHandler
{
    private readonly ScreenshotService screenshotService = screenshotService
        ? screenshotService
        : throw new ArgumentNullException(nameof(screenshotService));

    private readonly InputConfiguration inputConfiguration = inputConfiguration
        ?? throw new ArgumentNullException(nameof(inputConfiguration));

    public bool Active { get; } = true;

    public void CheckInput()
    {
        if (inputConfiguration[Shortcut.Screenshot].IsDown())
            screenshotService.TakeScreenshotToFile(new());
    }
}
