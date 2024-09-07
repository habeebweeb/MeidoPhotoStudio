using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Message window input handler.</summary>
public partial class MessageWindow
{
    public class InputHandler(MessageWindow messageWindow, InputConfiguration inputConfiguration) : IInputHandler
    {
        private readonly MessageWindow messageWindow = messageWindow
            ?? throw new ArgumentNullException(nameof(messageWindow));

        private readonly InputConfiguration inputConfiguration = inputConfiguration
            ?? throw new ArgumentNullException(nameof(inputConfiguration));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.ToggleMessageWindow].IsDown())
                messageWindow.ToggleVisibility();
        }
    }
}
