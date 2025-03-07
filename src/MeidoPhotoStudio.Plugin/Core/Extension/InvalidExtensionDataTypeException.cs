namespace MeidoPhotoStudio.Plugin.Core.Extension;

[Serializable]
public class InvalidExtensionDataTypeException : Exception
{
    public InvalidExtensionDataTypeException()
        : base("Scene aspect extension data type is not valid")
    {
    }

    public InvalidExtensionDataTypeException(string message)
        : base(message)
    {
    }

    public InvalidExtensionDataTypeException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
