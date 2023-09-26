namespace MeidoPhotoStudio.Plugin;

// TODO: IManager? I hardly know her! Managers that need updates should be a monobehaviour instead.
public interface IManager
{
    void Update();

    void Activate();

    void Deactivate();
}
