namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

[Serializable]
public class InvalidLoadOptionException : Exception
{
    public InvalidLoadOptionException(string message)
        : base(message)
    {
    }

    public InvalidLoadOptionException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
