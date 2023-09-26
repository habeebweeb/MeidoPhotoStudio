using System;

using MeidoPhotoStudio.Plugin.Service.Input;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Plugin core active toggle input handler.</summary>
public partial class PluginCore
{
    private class InputHandler : IInputHandler
    {
        private readonly PluginCore pluginCore;

        static InputHandler() =>
            InputManager.Register(MpsKey.Activate, KeyCode.F6, "Activate/deactivate MeidoPhotoStudio");

        public InputHandler(PluginCore pluginCore)
        {
            if (pluginCore == null)
                throw new ArgumentNullException(nameof(pluginCore));

            this.pluginCore = pluginCore;
        }

        public bool Active { get; } = true;

        public void CheckInput()
        {
            if (InputManager.GetKeyDown(MpsKey.Activate))
                pluginCore.ToggleActive();
        }
    }
}
