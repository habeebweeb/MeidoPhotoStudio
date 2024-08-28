namespace MeidoPhotoStudio.Plugin.Core.UndoRedo;

public interface ISettableTransactionalUndoRedo<in TValue> : ITransactionalUndoRedo<TValue>
    where TValue : struct
{
    void Set(TValue value);
}
