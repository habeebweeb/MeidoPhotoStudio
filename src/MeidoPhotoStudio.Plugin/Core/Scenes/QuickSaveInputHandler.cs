using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core.Scenes;

public class QuickSaveInputHandler(
    QuickSaveService quickSaveService,
    InputConfiguration inputConfiguration)
    : IInputHandler
{
    private readonly QuickSaveService quickSaveService = quickSaveService
        ?? throw new ArgumentNullException(nameof(quickSaveService));

    private readonly InputConfiguration inputConfiguration = inputConfiguration
        ?? throw new ArgumentNullException(nameof(inputConfiguration));

    public bool Active { get; } = true;

    public void CheckInput()
    {
        if (inputConfiguration[Shortcut.QuickSaveScene].IsDown())
            quickSaveService.QuickSave();
        else if (inputConfiguration[Shortcut.QuickLoadScene].IsDown())
            quickSaveService.QuickLoad();
    }
}
