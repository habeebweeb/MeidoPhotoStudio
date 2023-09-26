using System;

using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Scene window input handler.</summary>
public partial class SceneWindow
{
    public class InputHandler : IInputHandler
    {
        private readonly SceneWindow sceneWindow;

        static InputHandler() =>
            InputManager.Register(MpsKey.OpenSceneManager, UnityEngine.KeyCode.F8, "Hide/show scene manager");

        public InputHandler(SceneWindow sceneWindow) =>
            this.sceneWindow = sceneWindow ?? throw new ArgumentNullException(nameof(sceneWindow));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (InputManager.GetKeyDown(MpsKey.OpenSceneManager))
                sceneWindow.Visible = !sceneWindow.Visible;
        }
    }
}
