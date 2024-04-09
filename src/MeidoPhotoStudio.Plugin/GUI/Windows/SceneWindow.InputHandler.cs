using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Scene window input handler.</summary>
public partial class SceneWindow
{
    public class InputHandler : IInputHandler
    {
        private readonly SceneWindow sceneWindow;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(SceneWindow sceneWindow, InputConfiguration inputConfiguration)
        {
            this.sceneWindow = sceneWindow ?? throw new ArgumentNullException(nameof(sceneWindow));
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.ToggleSceneWindow].IsDown())
                sceneWindow.Visible = !sceneWindow.Visible;
        }
    }
}
