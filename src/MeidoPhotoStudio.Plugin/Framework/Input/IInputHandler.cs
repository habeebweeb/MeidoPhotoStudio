namespace MeidoPhotoStudio.Plugin.Framework.Input;

public interface IInputHandler
{
    bool Active { get; }

    void CheckInput();
}
