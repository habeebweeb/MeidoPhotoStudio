using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Scene manager input handler.</summary>
public partial class SceneManager
{
    public class InputHandler : IInputHandler
    {
        private readonly SceneManager sceneManager;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(SceneManager sceneManager, InputConfiguration inputConfiguration)
        {
            if (sceneManager is null)
                throw new ArgumentNullException(nameof(sceneManager));

            this.sceneManager = sceneManager;
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.QuickSaveScene].IsDown())
                sceneManager.QuickSaveScene();
            else if (inputConfiguration[Shortcut.QuickLoadScene].IsDown())
                sceneManager.QuickLoadScene();
        }
    }
}
