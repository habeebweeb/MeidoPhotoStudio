using System.Collections.Generic;

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class MenuDataBaseExtensions
{
    public static IEnumerator<MenuDataBase> GetEnumerator(this MenuDataBase menuDatabase)
    {
        if (!menuDatabase.JobFinished())
            throw new Menu.MenuDataBaseBusyException();

        var dataSize = menuDatabase.GetDataSize();

        for (var i = 0; i < dataSize; i++)
        {
            menuDatabase.SetIndex(i);

            yield return menuDatabase;
        }
    }

    public static MPN GetMpn(this MenuDataBase menuDataBase) =>
        (MPN)menuDataBase.GetCategoryMpn();
}
