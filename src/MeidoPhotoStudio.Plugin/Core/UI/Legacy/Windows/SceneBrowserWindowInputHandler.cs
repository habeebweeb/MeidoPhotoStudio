using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SceneBrowserWindowInputHandler(
    SceneBrowserWindow sceneBrowserWindow, InputConfiguration inputConfiguration) : IInputHandler
{
    private readonly SceneBrowserWindow sceneBrowserWindow = sceneBrowserWindow
        ?? throw new ArgumentNullException(nameof(sceneBrowserWindow));

    private readonly InputConfiguration inputConfiguration = inputConfiguration
        ?? throw new ArgumentNullException(nameof(inputConfiguration));

    public bool Active { get; } = true;

    public void CheckInput()
    {
        if (inputConfiguration[Shortcut.ToggleSceneWindow].IsDown())
            ToggleSceneBrowserVisible();

        void ToggleSceneBrowserVisible() =>
            sceneBrowserWindow.Visible = !sceneBrowserWindow.Visible;
    }
}
