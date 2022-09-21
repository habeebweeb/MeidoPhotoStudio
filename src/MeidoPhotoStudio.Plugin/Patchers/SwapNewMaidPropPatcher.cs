#if COM25
using System;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

namespace MeidoPhotoStudio.Plugin;

public static class SwapNewMaidPropPatcher
{
    public static event EventHandler<ProcStartEventArgs> NewMaidPropSwapping;

    [HarmonyPatch(typeof(Maid), nameof(Maid.SwapNewMaidProp))]
    [HarmonyPrefix]
    [SuppressMessage("StyleCop.Analyzers.NamingRules", "SA1313", Justification = "Harmony parameter")]
    private static void NotifyNewMaidPropSwapping(Maid __instance, bool toNewBody)
    {
        if (__instance.IsCrcBody == toNewBody)
            return;

        NewMaidPropSwapping?.Invoke(null, new(__instance));
    }
}
#endif
