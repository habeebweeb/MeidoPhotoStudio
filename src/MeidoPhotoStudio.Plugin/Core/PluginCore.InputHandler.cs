using System;

using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Plugin core active toggle input handler.</summary>
public partial class PluginCore
{
    private class InputHandler : IInputHandler
    {
        private readonly PluginCore pluginCore;
        private readonly InputConfiguration inputConfiguration;

        public InputHandler(PluginCore pluginCore, InputConfiguration inputConfiguration)
        {
            if (pluginCore == null)
                throw new ArgumentNullException(nameof(pluginCore));

            this.pluginCore = pluginCore;
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.ActivatePlugin].IsDown())
                pluginCore.ToggleActive();
        }
    }
}
