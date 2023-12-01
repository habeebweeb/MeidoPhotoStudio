using System;

namespace MeidoPhotoStudio.Plugin.Framework.Menu;

[Serializable]
internal class MenuDataBaseBusyException : Exception
{
    public MenuDataBaseBusyException()
        : base($"{nameof(MenuDataBase)} is busy.")
    {
    }

    public MenuDataBaseBusyException(string message)
        : base(message)
    {
    }

    public MenuDataBaseBusyException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
