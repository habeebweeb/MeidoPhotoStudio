using System;

using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Main window input handler.</summary>
public partial class MainWindow
{
    public class InputHandler : IInputHandler
    {
        private readonly MainWindow mainWindow;

        static InputHandler() =>
            InputManager.Register(MpsKey.ToggleUI, UnityEngine.KeyCode.Tab, "Show/hide all UI");

        public InputHandler(MainWindow mainWindow) =>
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (InputManager.GetKeyDown(MpsKey.ToggleUI))
                mainWindow.Visible = !mainWindow.Visible;
        }
    }
}
