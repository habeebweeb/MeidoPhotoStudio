namespace MeidoPhotoStudio.Plugin.Core.UndoRedo;

public interface ITransactionalUndoRedo<in TValue>
    where TValue : struct
{
    void StartChange();

    void EndChange();

    void Cancel();
}
