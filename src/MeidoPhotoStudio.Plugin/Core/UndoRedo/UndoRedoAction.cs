namespace MeidoPhotoStudio.Plugin.Core.UndoRedo;

public class UndoRedoAction(Action undoAction, Action redoAction) : IUndoRedo
{
    private readonly Action undoAction = undoAction ?? throw new ArgumentNullException(nameof(undoAction));
    private readonly Action redoAction = redoAction ?? throw new ArgumentNullException(nameof(redoAction));

    public void Undo() =>
        undoAction?.Invoke();

    public void Redo() =>
        redoAction?.Invoke();
}
