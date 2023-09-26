using System;

using MeidoPhotoStudio.Plugin.Service.Input;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Meido manager input handler.</summary>
public partial class MeidoManager
{
    public class InputHandler : IInputHandler
    {
        private readonly MeidoManager meidoManager;

        static InputHandler() =>
            InputManager.Register(MpsKey.MeidoUndressing, KeyCode.H, "All maid undressing");

        public InputHandler(MeidoManager meidoManager) =>
            this.meidoManager = meidoManager ?? throw new ArgumentNullException(nameof(meidoManager));

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (InputManager.GetKeyDown(MpsKey.MeidoUndressing))
                meidoManager.UndressAll();
        }
    }
}
