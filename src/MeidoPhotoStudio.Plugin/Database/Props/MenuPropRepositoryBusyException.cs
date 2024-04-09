namespace MeidoPhotoStudio.Database.Props.Menu;

[Serializable]
internal class MenuPropRepositoryBusyException : Exception
{
    public MenuPropRepositoryBusyException()
        : base($"{nameof(MenuPropRepository)} is busy.")
    {
    }

    public MenuPropRepositoryBusyException(string message)
        : base(message)
    {
    }

    public MenuPropRepositoryBusyException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
