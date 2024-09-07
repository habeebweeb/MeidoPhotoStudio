using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UndoRedo;
using MeidoPhotoStudio.Plugin.Framework.Input;

namespace MeidoPhotoStudio.Plugin.Core;

public class UndoRedoInputHandler(UndoRedoService undoRedoService, InputConfiguration inputConfiguration)
    : IInputHandler
{
    private readonly UndoRedoService undoRedoService = undoRedoService
        ?? throw new ArgumentNullException(nameof(undoRedoService));

    private readonly InputConfiguration inputConfiguration = inputConfiguration
        ?? throw new ArgumentNullException(nameof(inputConfiguration));

    public bool Active { get; } = true;

    public void CheckInput()
    {
        if (inputConfiguration[Shortcut.Undo].IsDown())
            undoRedoService.Undo();
        else if (inputConfiguration[Shortcut.Redo].IsDown())
            undoRedoService.Redo();
    }
}
