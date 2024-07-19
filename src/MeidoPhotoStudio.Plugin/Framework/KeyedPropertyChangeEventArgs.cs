namespace MeidoPhotoStudio.Plugin.Framework;

public class KeyedPropertyChangeEventArgs<T>(T key) : EventArgs
{
    public T Key { get; } = key;
}
