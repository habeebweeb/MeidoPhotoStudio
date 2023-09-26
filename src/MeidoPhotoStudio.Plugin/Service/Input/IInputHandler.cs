namespace MeidoPhotoStudio.Plugin.Service.Input;

public interface IInputHandler
{
    bool Active { get; }

    void CheckInput();
}
