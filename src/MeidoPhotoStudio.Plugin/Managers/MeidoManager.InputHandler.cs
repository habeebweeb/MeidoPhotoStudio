using System;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Meido manager input handler.</summary>
public partial class MeidoManager
{
    public class InputHandler : IInputHandler
    {
        private readonly MeidoManager meidoManager;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(MeidoManager meidoManager, InputConfiguration inputConfiguration)
        {
            this.meidoManager = meidoManager ?? throw new ArgumentNullException(nameof(meidoManager));
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.CycleMaidDressing].IsDown())
                meidoManager.UndressAll();
        }
    }
}
