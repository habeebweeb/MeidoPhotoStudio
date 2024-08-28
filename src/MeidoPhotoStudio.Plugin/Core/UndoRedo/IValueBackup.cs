namespace MeidoPhotoStudio.Plugin.Core.UndoRedo;

public interface IValueBackup<T> : IEquatable<IValueBackup<T>>
{
    void Apply(T @object);

    bool Equals(object startingState);
}
