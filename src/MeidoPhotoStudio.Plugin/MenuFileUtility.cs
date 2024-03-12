using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MeidoPhotoStudio.Plugin;

public static class MenuFileUtility
{
    public const string NoCategory = "noCategory";

    public static readonly string[] MenuCategories =
    {
        NoCategory, "acchat", "headset", "wear", "skirt", "onepiece", "mizugi", "bra", "panz", "stkg", "shoes",
        "acckami", "megane", "acchead", "acchana", "accmimi", "glove", "acckubi", "acckubiwa", "acckamisub", "accnip",
        "accude", "accheso", "accashi", "accsenaka", "accshippo", "accxxx",
    };

    private static readonly HashSet<string> AccMpn = new(StringComparer.InvariantCultureIgnoreCase);

    static MenuFileUtility()
    {
        AccMpn.UnionWith(MenuCategories.Skip(1));
        GameMain.Instance.StartCoroutine(CheckMenuDataBaseJob());
    }

    public static event EventHandler MenuFilesReadyChange;

    public static bool MenuFilesReady { get; private set; }

    private static IEnumerator CheckMenuDataBaseJob()
    {
        if (MenuFilesReady)
            yield break;

        while (!GameMain.Instance.MenuDataBase.JobFinished())
            yield return null;

        MenuFilesReady = true;
        MenuFilesReadyChange?.Invoke(null, EventArgs.Empty);
    }
}
