using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Message window input handler.</summary>
public partial class MessageWindow
{
    public class InputHandler : IInputHandler
    {
        private readonly MessageWindow messageWindow;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(MessageWindow messageWindow, InputConfiguration inputConfiguration)
        {
            this.messageWindow = messageWindow ?? throw new ArgumentNullException(nameof(messageWindow));
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.ToggleMessageWindow].IsDown())
                messageWindow.ToggleVisibility();
        }
    }
}
