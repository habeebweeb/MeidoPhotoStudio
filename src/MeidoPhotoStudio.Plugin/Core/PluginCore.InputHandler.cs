using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework.Input;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Plugin core active toggle input handler.</summary>
public partial class PluginCore
{
    private class InputHandler : IInputHandler
    {
        private readonly PluginCore pluginCore;
        private readonly InputConfiguration inputConfiguration;
        private readonly CustomMaidSceneService customMaidSceneService;

        public InputHandler(PluginCore pluginCore, InputConfiguration inputConfiguration, CustomMaidSceneService customMaidSceneService)
        {
            if (pluginCore == null)
                throw new ArgumentNullException(nameof(pluginCore));

            this.pluginCore = pluginCore;
            this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
            this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        }

        public bool Active =>
            customMaidSceneService.ValidScene;

        public void CheckInput()
        {
            if (inputConfiguration[Shortcut.ActivatePlugin].IsDown())
                pluginCore.ToggleActive();
        }
    }
}
