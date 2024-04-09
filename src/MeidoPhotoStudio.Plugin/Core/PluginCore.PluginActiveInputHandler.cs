using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Input handler wrapper that's active when the plugin core is active.</summary>
public partial class PluginCore
{
    private class PluginActiveInputHandler<T> : IInputHandler
        where T : IInputHandler
    {
        private readonly PluginCore pluginCore;
        private readonly T inputHandler;

        public PluginActiveInputHandler(PluginCore pluginCore, T inputHandler)
        {
            if (pluginCore == null)
                throw new ArgumentNullException(nameof(pluginCore));

            if (inputHandler == null)
                throw new ArgumentNullException(nameof(inputHandler));

            this.pluginCore = pluginCore;
            this.inputHandler = inputHandler;
        }

        public bool Active =>
            pluginCore.active && inputHandler.Active;

        public void CheckInput() =>
            inputHandler.CheckInput();
    }
}
