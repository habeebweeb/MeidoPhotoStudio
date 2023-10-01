using System;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Main window input handler.</summary>
public partial class MainWindow
{
    public class InputHandler : IInputHandler
    {
        private readonly MainWindow mainWindow;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(MainWindow mainWindow, InputConfiguration inputConfiguration)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.ToggleMainWindow].IsDown())
                mainWindow.Visible = !mainWindow.Visible;
        }
    }
}
