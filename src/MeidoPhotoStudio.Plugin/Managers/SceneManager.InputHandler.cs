using System;

using MeidoPhotoStudio.Plugin.Service.Input;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Scene manager input handler.</summary>
public partial class SceneManager
{
    public class InputHandler : IInputHandler
    {
        private readonly SceneManager sceneManager;

        static InputHandler()
        {
            InputManager.Register(MpsKey.SaveScene, KeyCode.S, "Quick save scene");
            InputManager.Register(MpsKey.LoadScene, KeyCode.A, "Load quick saved scene");
        }

        public InputHandler(SceneManager sceneManager)
        {
            if (sceneManager is null)
                throw new ArgumentNullException(nameof(sceneManager));

            this.sceneManager = sceneManager;
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (!InputManager.Control)
                return;

            if (InputManager.GetKeyDown(MpsKey.SaveScene))
                sceneManager.QuickSaveScene();
            else if (InputManager.GetKeyDown(MpsKey.LoadScene))
                sceneManager.QuickLoadScene();
        }
    }
}
