namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public interface IVirtualListHandler
{
    int Count { get; }

    Vector2 ItemDimensions(int index);
}
