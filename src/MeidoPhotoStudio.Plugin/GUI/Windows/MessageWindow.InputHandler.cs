using System;

using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Message window input handler.</summary>
public partial class MessageWindow
{
    public class InputHandler : IInputHandler
    {
        private readonly MessageWindow messageWindow;

        static InputHandler() =>
            InputManager.Register(MpsKey.ToggleMessage, UnityEngine.KeyCode.M, "Show/hide message box");

        public InputHandler(MessageWindow messageWindow) =>
            this.messageWindow = messageWindow ?? throw new ArgumentNullException(nameof(messageWindow));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (InputManager.GetKeyDown(MpsKey.ToggleMessage))
                messageWindow.ToggleVisibility();
        }
    }
}
