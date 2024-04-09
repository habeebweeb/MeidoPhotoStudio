using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Screenshot service input handler.</summary>
public partial class ScreenshotService
{
    public class InputHandler : IInputHandler
    {
        private readonly ScreenshotService screenshotService;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(ScreenshotService screenshotService, InputConfiguration inputConfiguration)
        {
            this.screenshotService = screenshotService
                ? screenshotService
                : throw new ArgumentNullException(nameof(screenshotService));

            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.Screenshot].IsDown())
                screenshotService.TakeScreenshotToFile();
        }
    }
}
