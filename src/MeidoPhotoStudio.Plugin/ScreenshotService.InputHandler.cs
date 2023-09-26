using System;

using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Screenshot service input handler.</summary>
public partial class ScreenshotService
{
    public class InputHandler : IInputHandler
    {
        private readonly ScreenshotService screenshotService;

        static InputHandler() =>
            InputManager.Register(MpsKey.Screenshot, UnityEngine.KeyCode.S, "Take screenshot");

        public InputHandler(ScreenshotService screenshotService) =>
            this.screenshotService = screenshotService
                ? screenshotService
                : throw new ArgumentNullException(nameof(screenshotService));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (!InputManager.Control && !InputManager.GetKey(MpsKey.CameraLayer) && InputManager.GetKeyDown(MpsKey.Screenshot))
                screenshotService.TakeScreenshotToFile();
        }
    }
}
