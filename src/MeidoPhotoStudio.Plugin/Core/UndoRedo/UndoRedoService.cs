namespace MeidoPhotoStudio.Plugin.Core.UndoRedo;

public class UndoRedoService
{
    private readonly Stack<IUndoRedo> undoStack = [];
    private readonly Stack<IUndoRedo> redoStack = [];

    public void Push(IUndoRedo undoRedo)
    {
        _ = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));

        undoStack.Push(undoRedo);
        redoStack.Clear();
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
    }

    public void Undo()
    {
        if (!undoStack.Any())
            return;

        var undoRedo = undoStack.Peek();

        undoRedo.Undo();

        redoStack.Push(undoStack.Pop());
    }

    public void Redo()
    {
        if (!redoStack.Any())
            return;

        var undoRedo = redoStack.Peek();

        undoRedo.Redo();

        undoStack.Push(redoStack.Pop());
    }
}
